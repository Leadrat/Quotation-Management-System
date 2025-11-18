import { formatCurrency } from "@/utils/quotationFormatter";

export interface LineItem {
  lineItemId?: string;
  itemName: string;
  description: string;
  quantity: number;
  unitRate: number;
  amount: number;
}

interface LineItemRepeaterProps {
  lineItems: LineItem[];
  onUpdate: (index: number, field: keyof LineItem, value: any) => void;
  onAdd: () => void;
  onRemove: (index: number) => void;
  disabled?: boolean;
}

export default function LineItemRepeater({
  lineItems,
  onUpdate,
  onAdd,
  onRemove,
  disabled = false,
}: LineItemRepeaterProps) {
  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <label className="block text-black dark:text-white">Line Items *</label>
        {!disabled && (
          <button
            type="button"
            onClick={onAdd}
            className="rounded bg-primary px-4 py-2 text-sm text-white hover:bg-opacity-90"
          >
            Add Line Item
          </button>
        )}
      </div>
      <div className="space-y-4">
        {lineItems.map((item, index) => (
          <div key={index} className="rounded border border-stroke p-4 dark:border-strokedark">
            <div className="mb-4 flex items-center justify-between">
              <span className="font-medium text-black dark:text-white">Item {index + 1}</span>
              {!disabled && lineItems.length > 1 && (
                <button
                  type="button"
                  onClick={() => onRemove(index)}
                  className="rounded bg-red-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                >
                  Remove
                </button>
              )}
            </div>
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <div>
                <label className="mb-2.5 block text-sm text-black dark:text-white">Item Name *</label>
                <input
                  type="text"
                  value={item.itemName}
                  onChange={(e) => onUpdate(index, "itemName", e.target.value)}
                  required
                  disabled={disabled}
                  className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white disabled:opacity-50"
                />
              </div>
              <div>
                <label className="mb-2.5 block text-sm text-black dark:text-white">Description</label>
                <input
                  type="text"
                  value={item.description}
                  onChange={(e) => onUpdate(index, "description", e.target.value)}
                  disabled={disabled}
                  className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white disabled:opacity-50"
                />
              </div>
              <div>
                <label className="mb-2.5 block text-sm text-black dark:text-white">Quantity *</label>
                <input
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={item.quantity}
                  onChange={(e) => onUpdate(index, "quantity", parseFloat(e.target.value) || 0)}
                  required
                  disabled={disabled}
                  className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white disabled:opacity-50"
                />
              </div>
              <div>
                <label className="mb-2.5 block text-sm text-black dark:text-white">Unit Rate *</label>
                <input
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={item.unitRate}
                  onChange={(e) => onUpdate(index, "unitRate", parseFloat(e.target.value) || 0)}
                  required
                  disabled={disabled}
                  className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white disabled:opacity-50"
                />
              </div>
            </div>
            <div className="mt-2 text-right text-sm font-medium text-black dark:text-white">
              Amount: {formatCurrency(item.amount)}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

