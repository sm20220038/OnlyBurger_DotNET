// A small horizontal stepper that visualizes an order's delivery progress.
// Mirrors the backend DeliveryStatus enum: Pending -> Preparing -> OutForDelivery -> Delivered.
const STEPS = [
  { key: 'Preparing', label: 'Preparing' },
  { key: 'OutForDelivery', label: 'On the way' },
  { key: 'Delivered', label: 'Delivered' },
]

const ORDER = ['Pending', 'Preparing', 'OutForDelivery', 'Delivered']

export default function DeliveryTracker({ status }) {
  const currentIndex = ORDER.indexOf(status)

  return (
    <div className="delivery-tracker" aria-label={`Delivery status: ${status}`}>
      {STEPS.map((step) => {
        const stepIndex = ORDER.indexOf(step.key)
        const state =
          stepIndex < currentIndex ? 'done' : stepIndex === currentIndex ? 'active' : 'todo'
        return (
          <div key={step.key} className={`delivery-step delivery-step-${state}`}>
            <span className="delivery-dot" />
            <span className="delivery-label">{step.label}</span>
          </div>
        )
      })}
    </div>
  )
}
