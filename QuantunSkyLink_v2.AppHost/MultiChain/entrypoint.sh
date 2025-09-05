#!/bin/bash
# MultiChain node entrypoint script

set -e

# Set up MultiChain based on role
if [ "$MULTICHAIN_ROLE" = "master" ]; then
    # Check if chain exists
    if [ ! -d "/root/.multichain/$MULTICHAIN_CHAIN" ]; then
        echo "Initializing master node..."
        
        # Create the chain with specific parameters
        multichain-util create $MULTICHAIN_CHAIN \
            -anyone-can-connect=false \
            -mining-diversity=0.3 \
            -admin-consensus-upgrade=0.5 \
            -admin-consensus-admin=0.7 \
            -admin-consensus-activate=0.5 \
            -mining-requires-peers=true
        
        # Start the chain with proper network binding for both RPC and P2P
        multichaind $MULTICHAIN_CHAIN -daemon -rpcbind=0.0.0.0 -rpcallowip=0.0.0.0/0 -port=$MULTICHAIN_P2P_PORT
        
        # Wait for chain to start
        sleep 10
        
        # Get the node's address and grant permissions to self
        NODE_ADDRESS=$(multichain-cli $MULTICHAIN_CHAIN getaddresses | jq -r '.[0]')
        if [ "$NODE_ADDRESS" != "null" ] && [ ! -z "$NODE_ADDRESS" ]; then
            echo "Granting permissions to node address: $NODE_ADDRESS"
            multichain-cli $MULTICHAIN_CHAIN grant $NODE_ADDRESS connect,send,receive,issue,create,mine,admin
        else
            echo "Warning: Could not get node address, using default permissions"
        fi
    else
        echo "Starting existing master node..."
        multichaind $MULTICHAIN_CHAIN -daemon -rpcbind=0.0.0.0 -rpcallowip=0.0.0.0/0 -port=$MULTICHAIN_P2P_PORT
    fi
elif [ "$MULTICHAIN_ROLE" = "validator" ]; then
    # Wait for master node to be available and ready
    echo "Waiting for master node at $MULTICHAIN_MASTER_NODE:$MULTICHAIN_P2P_PORT..."
    
    # Enhanced connectivity check - verify both P2P and RPC ports
    echo "Checking master node P2P port ($MULTICHAIN_P2P_PORT)..."
    until nc -z $MULTICHAIN_MASTER_NODE $MULTICHAIN_P2P_PORT; do
        echo "Master node P2P port not yet available, waiting..."
        sleep 5
    done
    
    echo "P2P port is open, checking RPC port ($MULTICHAIN_RPC_PORT)..."
    until nc -z $MULTICHAIN_MASTER_NODE $MULTICHAIN_RPC_PORT; do
        echo "Master node RPC port not yet available, waiting..."
        sleep 5
    done
    
    echo "Both ports are open, waiting for chain to be fully ready..."
    
    # Additional wait to ensure master node is fully initialized
    sleep 15
    
    # Try to get master node info to ensure it's ready
    MASTER_IP=$(getent hosts $MULTICHAIN_MASTER_NODE | awk '{ print $1 }')
    echo "Master node IP resolved to: $MASTER_IP"
    
    # Check if chain exists
    if [ ! -d "/root/.multichain/$MULTICHAIN_CHAIN" ]; then
        echo "Connecting validator to master node..."
        
        # Try multiple connection attempts with backoff
        for i in {1..5}; do
            echo "Connection attempt $i/5..."
            if multichaind $MULTICHAIN_CHAIN@$MASTER_IP:$MULTICHAIN_P2P_PORT -daemon; then
                echo "Successfully connected to master node"
                break
            else
                echo "Connection attempt $i failed, retrying in 10 seconds..."
                sleep 10
            fi
            
            if [ $i -eq 5 ]; then
                echo "Failed to connect after 5 attempts"
                exit 1
            fi
        done
    else
        echo "Starting existing validator node..."
        multichaind $MULTICHAIN_CHAIN -daemon
    fi
else
    echo "Unknown role: $MULTICHAIN_ROLE"
    exit 1
fi

# Configure RPC settings
if [ ! -f "/root/.multichain/$MULTICHAIN_CHAIN/multichain.conf" ]; then
    echo "Configuring RPC settings..."
    
    # Use provided password or generate a secure default
    RPC_PASSWORD=${MULTICHAIN_RPC_PASSWORD:-$(openssl rand -hex 16)}
    
    cat > /root/.multichain/$MULTICHAIN_CHAIN/multichain.conf << EOF
rpcuser=$MULTICHAIN_RPC_USER
rpcpassword=$RPC_PASSWORD
rpcallowip=0.0.0.0/0
rpcport=$MULTICHAIN_RPC_PORT
EOF

    echo "RPC configured with user: $MULTICHAIN_RPC_USER"
    
    # Restart the chain to apply settings
    multichain-cli $MULTICHAIN_CHAIN stop
    sleep 5
    if [ "$MULTICHAIN_ROLE" = "master" ]; then
        multichaind $MULTICHAIN_CHAIN -daemon -rpcbind=0.0.0.0 -rpcallowip=0.0.0.0/0 -port=$MULTICHAIN_P2P_PORT
    else
        multichaind $MULTICHAIN_CHAIN -daemon
    fi
fi

# Set up P2P access control
if [ "$MULTICHAIN_ROLE" = "master" ]; then
    echo "Setting up P2P access control..."
    
    # Wait for chain to start
    sleep 5
    
    # Allow only specific nodes to connect
    if [ ! -z "$MULTICHAIN_ALLOWED_NODES" ]; then
        IFS=',' read -ra NODES <<< "$MULTICHAIN_ALLOWED_NODES"
        for NODE in "${NODES[@]}"; do
            echo "Allowing node: $NODE"
            multichain-cli $MULTICHAIN_CHAIN grant $NODE connect,send,receive
        done
    fi
fi

# Keep container running and log blockchain activity
echo "Node started successfully. Tailing logs..."
tail -f /root/.multichain/$MULTICHAIN_CHAIN/debug.log
