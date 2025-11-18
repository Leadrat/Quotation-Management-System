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
}

export function calculateQuotationTotals(
  lineItems: LineItem[],
  discountPercentage: number,
  clientStateCode?: string | null,
  companyStateCode?: string
): QuotationTotals {
  // Calculate subtotal from line items
  const subtotal = lineItems.reduce((sum, item) => {
    const amount = item.amount ?? item.quantity * item.unitRate;
    return sum + amount;
  }, 0);

  // Calculate discount
  const discountAmount = subtotal * (discountPercentage / 100);
  const finalDiscountAmount = Math.min(discountAmount, subtotal); // Discount cannot exceed subtotal

  // Calculate tax
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

