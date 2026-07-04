// Inline feedback banner. type: "error" | "success" | "info".
export default function Message({ type = 'info', children }) {
  if (!children) {
    return null
  }
  return <div className={`message message-${type}`}>{children}</div>
}
