import { formatCurrency } from "@/utils/quotationFormatter";
import type { BillingCycle } from "@/types/products";

interface LineItem {
  lineItemId?: string;
  sequenceNumber?: number;
  itemName: string;
  description?: string;
  quantity: number;
  unitRate: number;
  amount: number;
  productId?: string;
  productName?: string;
  productType?: string;
  billingCycle?: BillingCycle;
  hours?: number;
  originalProductPrice?: number;
  discountAmount?: number;
  taxCategoryId?: string;
  taxCategoryName?: string;
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
            {lineItems.some(item => item.billingCycle) && (
              <th className="px-4 py-3 font-medium text-black dark:text-white">Billing Cycle</th>
            )}
            {lineItems.some(item => item.hours) && (
              <th className="px-4 py-3 font-medium text-black dark:text-white">Hours</th>
            )}
            <th className="px-4 py-3 font-medium text-black dark:text-white">Unit Rate</th>
            {lineItems.some(item => item.discountAmount) && (
              <th className="px-4 py-3 font-medium text-black dark:text-white">Discount</th>
            )}
            <th className="px-4 py-3 font-medium text-black dark:text-white">Amount</th>
            {showActions && <th className="px-4 py-3 font-medium text-black dark:text-white">Actions</th>}
          </tr>
        </thead>
        <tbody>
          {lineItems.map((item, index) => (
            <tr key={item.lineItemId || index} className="border-b border-[#eee] dark:border-strokedark">
              <td className="px-4 py-3 text-black dark:text-white">{item.sequenceNumber || index + 1}</td>
              <td className="px-4 py-3 text-black dark:text-white">
                <div>
                  {item.itemName}
                  {item.productId && (
                    <span className="ml-2 text-xs text-gray-500">
                      {item.productType && `(${item.productType})`}
                    </span>
                  )}
                </div>
              </td>
              <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{item.description || "-"}</td>
              <td className="px-4 py-3 text-black dark:text-white">{item.quantity}</td>
              {lineItems.some(i => i.billingCycle) && (
                <td className="px-4 py-3 text-black dark:text-white">
                  {item.billingCycle || "-"}
                </td>
              )}
              {lineItems.some(i => i.hours) && (
                <td className="px-4 py-3 text-black dark:text-white">
                  {item.hours ? `${item.hours} hrs` : "-"}
                </td>
              )}
              <td className="px-4 py-3 text-black dark:text-white">
                {formatCurrency(item.unitRate)}
                {item.originalProductPrice && item.originalProductPrice !== item.unitRate && (
                  <div className="text-xs text-gray-500 line-through">
                    {formatCurrency(item.originalProductPrice)}
                  </div>
                )}
              </td>
              {lineItems.some(i => i.discountAmount) && (
                <td className="px-4 py-3 text-black dark:text-white">
                  {item.discountAmount ? formatCurrency(item.discountAmount) : "-"}
                </td>
              )}
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

