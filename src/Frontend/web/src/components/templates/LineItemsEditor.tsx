"use client";
import { formatCurrency } from "@/utils/quotationFormatter";
import type { CreateTemplateLineItemRequest, UpdateTemplateLineItemRequest } from "@/types/templates";

interface LineItemsEditorProps {
  lineItems: (CreateTemplateLineItemRequest | UpdateTemplateLineItemRequest)[];
  onUpdate: (index: number, field: keyof CreateTemplateLineItemRequest, value: any) => void;
  onAdd: () => void;
  onRemove: (index: number) => void;
  disabled?: boolean;
}

export default function LineItemsEditor({
  lineItems,
  onUpdate,
  onAdd,
  onRemove,
  disabled = false,
}: LineItemsEditorProps) {
  const calculateAmount = (item: CreateTemplateLineItemRequest | UpdateTemplateLineItemRequest) => {
    return (item.quantity || 0) * (item.unitRate || 0);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h5 className="text-lg font-semibold text-black dark:text-white">Line Items</h5>
        {!disabled && (
          <button
            type="button"
            onClick={onAdd}
            className="rounded bg-primary px-4 py-2 text-sm text-white hover:bg-opacity-90"
          >
            Add Item
          </button>
        )}
      </div>
      <div className="space-y-4">
        {lineItems.map((item, index) => {
          const amount = calculateAmount(item);
          return (
            <div
              key={index}
              className="rounded border border-stroke bg-white p-4 dark:border-strokedark dark:bg-boxdark"
            >
              <div className="mb-2 flex items-center justify-between">
                <span className="text-sm font-medium text-black dark:text-white">Item #{index + 1}</span>
                {!disabled && lineItems.length > 1 && (
                  <button
                    type="button"
                    onClick={() => onRemove(index)}
                    className="rounded bg-red-500 px-2 py-1 text-xs text-white hover:bg-opacity-90"
                  >
                    Remove
                  </button>
                )}
              </div>
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div className="md:col-span-2">
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
                <div className="md:col-span-2">
                  <label className="mb-2.5 block text-sm text-black dark:text-white">Description</label>
                  <textarea
                    value={item.description || ""}
                    onChange={(e) => onUpdate(index, "description", e.target.value)}
                    disabled={disabled}
                    rows={2}
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
                Amount: {formatCurrency(amount)}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

