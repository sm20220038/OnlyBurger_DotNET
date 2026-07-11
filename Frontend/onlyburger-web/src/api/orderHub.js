// SignalR connection to the backend's order-tracking hub.
//
// The customer's browser opens one connection and listens for `OrderUpdated` events. The
// backend pushes the full order (as an OrderDto) whenever an admin approves/rejects it or
// advances its delivery status, so the UI can update live without polling.

import { HubConnectionBuilder, HttpTransportType, LogLevel } from '@microsoft/signalr'
import { getToken } from './client'

const HUB_URL = '/hubs/orders'

/**
 * Builds (but does not start) a hub connection that authenticates with the current JWT.
 * The token is supplied via `accessTokenFactory`; SignalR sends it as the `access_token`
 * query-string value, which the backend reads on the hub handshake.
 */
export function createOrderHubConnection() {
  return new HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => getToken() ?? '',
      // Skip the long-polling fallback negotiation quirks in dev; WebSockets are proxied.
      transport: HttpTransportType.WebSockets,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}

export const ORDER_UPDATED_EVENT = 'OrderUpdated'
export const ADMIN_ORDERS_CHANGED_EVENT = 'AdminOrdersChanged'
