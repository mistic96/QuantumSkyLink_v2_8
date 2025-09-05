import { createClient } from '@base44/sdk';
// import { getAccessToken } from '@base44/sdk/utils/auth-utils';

// Create a client without authentication required
export const base44 = createClient({
  appId: "687b1c08d09b990edcb09fa1", 
  requiresAuth: false // Disable authentication for all operations
});
