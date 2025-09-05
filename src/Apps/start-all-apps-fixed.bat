@echo off
echo Starting all QuantumSkyLink Web Applications...

echo.
echo Starting BlockchainExplorer on port 5173...
start cmd /k "cd BlockchainExplorer\quantum-ledger-1452996e && npm run dev"

echo.
echo Starting LiquidityProvider on port 5174...
start cmd /k "cd LiquidityProvider\quantum-flow-ef2039a0 && npm run dev -- --port 5174"

echo.
echo Starting ManagementPortal on port 5175...
start cmd /k "cd ManagementPortal\quantum-sky-link-admin-console-a1e95f2e && npm run dev -- --port 5175"

echo.
echo Starting TokenPortal on port 5176...
start cmd /k "cd TokenPortal\quantum-mint-dcb09fa1 && npm run dev -- --port 5176"

echo.
echo All applications started!
echo BlockchainExplorer: http://localhost:5173
echo LiquidityProvider: http://localhost:5174
echo ManagementPortal: http://localhost:5175
echo TokenPortal: http://localhost:5176