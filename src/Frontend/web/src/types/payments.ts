export enum PaymentStatus {
  Pending = 0,
  Processing = 1,
  Success = 2,
  Failed = 3,
  Refunded = 4,
  PartiallyRefunded = 5,
  Cancelled = 6,
}

export enum PaymentGateway {
  Stripe = 0,
  Razorpay = 1,
  PayPal = 2,
  Custom = 99,
}

export interface Payment {
  paymentId: string;
  quotationId: string;
  paymentGateway: string;
  paymentReference: string;
  amountPaid: number;
  currency: string;
  paymentStatus: PaymentStatus;
  statusLabel: string;
  paymentDate?: string;
  createdAt: string;
  updatedAt: string;
  failureReason?: string;
  isRefundable: boolean;
  refundAmount?: number;
  refundReason?: string;
  refundDate?: string;
  canBeRefunded: boolean;
  canBeCancelled: boolean;
  paymentUrl?: string;
}

export interface PaymentDto extends Payment {}

export interface InitiatePaymentRequest {
  quotationId: string;
  paymentGateway: string;
  amount?: number;
  currency?: string;
}

export interface RefundPaymentRequest {
  amount?: number;
  reason: string;
}

export interface UpdatePaymentStatusRequest {
  paymentReference: string;
  status: string;
  amount?: number;
  currency?: string;
  paymentDate?: string;
  failureReason?: string;
  metadata?: Record<string, string>;
}

export interface PaymentDashboardDto {
  summary: PaymentSummaryDto;
  recentPayments: PaymentDto[];
  statusCounts: PaymentStatusCountDto[];
}

export interface PaymentSummaryDto {
  totalPending: number;
  totalPaid: number;
  totalRefunded: number;
  totalFailed: number;
  pendingCount: number;
  paidCount: number;
  refundedCount: number;
  failedCount: number;
}

export interface PaymentStatusCountDto {
  status: string;
  count: number;
  totalAmount: number;
}

export interface PaymentGatewayConfigDto {
  configId: string;
  companyId?: string;
  gatewayName: string;
  enabled: boolean;
  isTestMode: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePaymentGatewayConfigRequest {
  companyId?: string;
  gatewayName: string;
  apiKey: string;
  apiSecret: string;
  webhookSecret?: string;
  enabled: boolean;
  isTestMode: boolean;
}

export interface UpdatePaymentGatewayConfigRequest {
  apiKey?: string;
  apiSecret?: string;
  webhookSecret?: string;
  enabled?: boolean;
  isTestMode?: boolean;
}

