import { formatCurrency } from "@/utils/quotationFormatter";

interface LineItem {
  lineItemId?: string;
  sequenceNumber?: number;
  itemName: string;
  description?: string;
  quantity: number;
  unitRate: number;
  amount: number;
}

interface QuotationLineItemsTableProps {
  lineItems: LineItem[];
  showActions?: boolean;
  onEdit?: (item: LineItem) => void;
  onDelete?: (item: LineItem) => void;
}

export default function QuotationLineItemsTable({
  lineItems,
  showActions = false,
  onEdit,
  onDelete,
}: QuotationLineItemsTableProps) {
  if (!lineItems || lineItems.length === 0) {
    return (
      <div className="rounded border border-stroke p-4 text-center text-gray-500 dark:border-strokedark">
        No line items
      </div>
    );
  }

  return (
    <div className="max-w-full overflow-x-auto">
      <table className="w-full table-auto">
        <thead>
          <tr className="bg-gray-2 text-left dark:bg-meta-4">
            <th className="px-4 py-3 font-medium text-black dark:text-white">#</th>
            <th className="px-4 py-3 font-medium text-black dark:text-white">Item Name</th>
            <th className="px-4 py-3 font-medium text-black dark:text-white">Description</th>
            <th className="px-4 py-3 font-medium text-black dark:text-white">Quantity</th>
            <th className="px-4 py-3 font-medium text-black dark:text-white">Unit Rate</th>
            <th className="px-4 py-3 font-medium text-black dark:text-white">Amount</th>
            {showActions && <th className="px-4 py-3 font-medium text-black dark:text-white">Actions</th>}
          </tr>
        </thead>
        <tbody>
          {lineItems.map((item, index) => (
            <tr key={item.lineItemId || index} className="border-b border-[#eee] dark:border-strokedark">
              <td className="px-4 py-3 text-black dark:text-white">{item.sequenceNumber || index + 1}</td>
              <td className="px-4 py-3 text-black dark:text-white">{item.itemName}</td>
              <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{item.description || "-"}</td>
              <td className="px-4 py-3 text-black dark:text-white">{item.quantity}</td>
              <td className="px-4 py-3 text-black dark:text-white">{formatCurrency(item.unitRate)}</td>
              <td className="px-4 py-3 font-medium text-black dark:text-white">{formatCurrency(item.amount)}</td>
              {showActions && (
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    {onEdit && (
                      <button
                        onClick={() => onEdit(item)}
                        className="rounded bg-yellow-500 px-2 py-1 text-xs text-white hover:bg-opacity-90"
                      >
                        Edit
                      </button>
                    )}
                    {onDelete && (
                      <button
                        onClick={() => onDelete(item)}
                        className="rounded bg-red-500 px-2 py-1 text-xs text-white hover:bg-opacity-90"
                      >
                        Delete
                      </button>
                    )}
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

