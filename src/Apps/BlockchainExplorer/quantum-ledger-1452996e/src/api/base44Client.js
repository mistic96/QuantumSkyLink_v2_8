import { createClient } from '@base44/sdk';
// import { getAccessToken } from '@base44/sdk/utils/auth-utils';

// Create a client without authentication required
export const base44 = createClient({
  appId: "687acd5ccf6afcf81452996e", 
  requiresAuth: false // Disable authentication for all operations
});
