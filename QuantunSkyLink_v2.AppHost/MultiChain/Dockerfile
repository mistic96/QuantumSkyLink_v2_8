# MultiChain Docker Image for QuantumSkyLink v2
FROM ubuntu:20.04

# Avoid interactive prompts during package installation
ENV DEBIAN_FRONTEND=noninteractive

# Install required packages
RUN apt-get update && apt-get install -y \
    wget \
    curl \
    netcat \
    jq \
    software-properties-common \
    && rm -rf /var/lib/apt/lists/*

# Install MultiChain
RUN cd /tmp && \
    wget https://www.multichain.com/download/multichain-2.3.3.tar.gz && \
    tar -xvzf multichain-2.3.3.tar.gz && \
    cd multichain-2.3.3 && \
    mv multichaind multichain-cli multichain-util /usr/local/bin && \
    cd / && \
    rm -rf /tmp/multichain-2.3.3*

# Create multichain user and directories
RUN useradd -m -s /bin/bash multichain && \
    mkdir -p /root/.multichain && \
    chown -R multichain:multichain /root/.multichain

# Set environment variables with defaults (excluding sensitive data)
ENV MULTICHAIN_CHAIN=quantumledger
ENV MULTICHAIN_RPC_USER=multichainrpc
ENV MULTICHAIN_RPC_PORT=7446
ENV MULTICHAIN_P2P_PORT=7447
ENV MULTICHAIN_ROLE=validator
# Note: MULTICHAIN_RPC_PASSWORD should be set at runtime for security

# Copy entrypoint script
COPY entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# Expose ports
EXPOSE 7446 7447

# Set working directory
WORKDIR /root

# Use entrypoint script
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
