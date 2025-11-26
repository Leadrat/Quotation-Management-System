/**
 * Tax calculation utility (mirrors backend TaxCalculationService logic)
 * Calculates CGST, SGST, IGST based on client location (intra-state vs inter-state)
 */

export interface TaxCalculationResult {
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  totalTax: number;
}

export interface TaxCalculationInput {
  subtotal: number;
  discountAmount: number;
  clientStateCode?: string | null;
  companyStateCode?: string;
}

const TAX_RATE = 0.18; // 18% GST
const INTRA_STATE_CGST_RATE = 0.09; // 9% CGST
const INTRA_STATE_SGST_RATE = 0.09; // 9% SGST

export function calculateTax(input: TaxCalculationInput): TaxCalculationResult {
  const { subtotal, discountAmount, clientStateCode, companyStateCode = "27" } = input;
  const taxableAmount = subtotal - discountAmount;

  if (!clientStateCode) {
    // Default to inter-state if state code not provided
    return calculateInterStateTax(taxableAmount);
  }

  const isIntraState = clientStateCode.trim().toLowerCase() === companyStateCode.trim().toLowerCase();

  if (isIntraState) {
    return calculateIntraStateTax(taxableAmount);
  } else {
    return calculateInterStateTax(taxableAmount);
  }
}

function calculateIntraStateTax(taxableAmount: number): TaxCalculationResult {
  const cgst = taxableAmount * INTRA_STATE_CGST_RATE;
  const sgst = taxableAmount * INTRA_STATE_SGST_RATE;
  const totalTax = cgst + sgst;

  return {
    cgstAmount: cgst,
    sgstAmount: sgst,
    igstAmount: 0,
    totalTax,
  };
}

function calculateInterStateTax(taxableAmount: number): TaxCalculationResult {
  const igst = taxableAmount * TAX_RATE;

  return {
    cgstAmount: 0,
    sgstAmount: 0,
    igstAmount: igst,
    totalTax: igst,
  };
}

/**
 * Calculate quotation totals (subtotal, discount, tax, total)
 */
export interface QuotationTotals {
  subtotal: number;
  discountAmount: number;
  discountPercentage: number;
  taxAmount: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  totalAmount: number;
}

export interface LineItem {
  quantity: number;
  unitRate: number;
  amount?: number;
  discountAmount?: number;
}

export function calculateQuotationTotals(
  lineItems: LineItem[],
  discountPercentage: number,
  clientStateCode?: string | null,
  companyStateCode?: string
): QuotationTotals {
  // Calculate subtotal from line items (product-level discounts already applied in amount)
  const subtotal = lineItems.reduce((sum, item) => {
    const amount = item.amount ?? item.quantity * item.unitRate;
    return sum + amount;
  }, 0);

  // Calculate product-level discounts
  const productLevelDiscounts = lineItems.reduce((sum, item) => {
    return sum + (item.discountAmount || 0);
  }, 0);

  // Calculate quotation-level discount (applied after product-level discounts)
  const quotationDiscountAmount = subtotal * (discountPercentage / 100);
  const finalQuotationDiscount = Math.min(quotationDiscountAmount, subtotal);

  // Total discount = product-level + quotation-level
  const totalDiscountAmount = productLevelDiscounts + finalQuotationDiscount;
  const finalDiscountAmount = Math.min(totalDiscountAmount, subtotal);

  // Calculate tax on taxable amount (after all discounts)
  const taxResult = calculateTax({
    subtotal,
    discountAmount: finalDiscountAmount,
    clientStateCode,
    companyStateCode,
  });

  // Calculate total
  const totalAmount = subtotal - finalDiscountAmount + taxResult.totalTax;

  return {
    subtotal,
    discountAmount: finalDiscountAmount,
    discountPercentage,
    taxAmount: taxResult.totalTax,
    cgstAmount: taxResult.cgstAmount,
    sgstAmount: taxResult.sgstAmount,
    igstAmount: taxResult.igstAmount,
    totalAmount,
  };
}

