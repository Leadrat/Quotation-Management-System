export enum RefundStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Processing = 'Processing',
  Completed = 'Completed',
  Failed = 'Failed',
  Reversed = 'Reversed'
}

export enum RefundReasonCode {
  CLIENT_REQUEST = 'CLIENT_REQUEST',
  ERROR = 'ERROR',
  DISCOUNT_ADJUSTMENT = 'DISCOUNT_ADJUSTMENT',
  CANCELLATION = 'CANCELLATION',
  DUPLICATE_PAYMENT = 'DUPLICATE_PAYMENT',
  OTHER = 'OTHER'
}

export enum AdjustmentType {
  DISCOUNT_CHANGE = 'DISCOUNT_CHANGE',
  AMOUNT_CORRECTION = 'AMOUNT_CORRECTION',
  TAX_CORRECTION = 'TAX_CORRECTION'
}

export enum AdjustmentStatus {
  PENDING = 'PENDING',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  APPLIED = 'APPLIED'
}

export enum RefundTimelineEventType {
  REQUESTED = 'REQUESTED',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  PROCESSING = 'PROCESSING',
  COMPLETED = 'COMPLETED',
  FAILED = 'FAILED',
  REVERSED = 'REVERSED'
}

export interface RefundDto {
  refundId: string;
  paymentId: string;
  quotationId: string;
  refundAmount: number;
  refundReason: string;
  refundReasonCode: RefundReasonCode;
  requestedByUserName: string;
  approvedByUserName?: string;
  refundStatus: RefundStatus;
  approvalLevel?: string;
  comments?: string;
  failureReason?: string;
  requestDate: string;
  approvalDate?: string;
  completedDate?: string;
  paymentGatewayReference?: string;
  reversedReason?: string;
  reversedDate?: string;
}

export interface RefundTimelineDto {
  timelineId: string;
  refundId: string;
  eventType: RefundTimelineEventType;
  actedByUserId: string;
  actedByUserName: string;
  comments?: string;
  eventDate: string;
  ipAddress?: string;
}

export interface RefundMetricsDto {
  totalRefundAmount: number;
  totalRefundCount: number;
  pendingRefundCount: number;
  refundPercentage: number;
  averageRefundAmount: number;
  averageTAT: number;
  reasonBreakdown: RefundReasonBreakdown[];
  statusBreakdown: RefundStatusBreakdown[];
  tatByPeriod: RefundTATByPeriod[];
}

export interface RefundReasonBreakdown {
  reasonCode: string;
  count: number;
  amount: number;
  percentage: number;
}

export interface RefundStatusBreakdown {
  status: string;
  count: number;
  percentage: number;
}

export interface RefundTATByPeriod {
  period: string;
  averageHours: number;
}

export interface CreateRefundRequest {
  paymentId: string;
  quotationId?: string;
  refundAmount?: number;
  refundReason: string;
  refundReasonCode: RefundReasonCode;
  comments?: string;
}

export interface ApproveRefundRequest {
  comments?: string;
}

export interface RejectRefundRequest {
  rejectionReason: string;
  comments?: string;
}

export interface ReverseRefundRequest {
  reversedReason: string;
  comments?: string;
}

export interface BulkProcessRefundsRequest {
  refundIds: string[];
  comments?: string;
}

export interface BulkProcessRefundsResult {
  totalProcessed: number;
  successCount: number;
  failureCount: number;
  results: BulkRefundResult[];
}

export interface BulkRefundResult {
  refundId: string;
  success: boolean;
  message: string;
}

export interface AdjustmentDto {
  adjustmentId: string;
  quotationId: string;
  adjustmentType: AdjustmentType;
  originalAmount: number;
  adjustedAmount: number;
  reason: string;
  requestedByUserName: string;
  approvedByUserName?: string;
  status: AdjustmentStatus;
  approvalLevel?: string;
  requestDate: string;
  approvalDate?: string;
  appliedDate?: string;
  adjustmentDifference: number;
}

export interface CreateAdjustmentRequest {
  quotationId: string;
  adjustmentType: AdjustmentType;
  originalAmount: number;
  adjustedAmount: number;
  reason: string;
}

export interface ApproveAdjustmentRequest {
  comments?: string;
}

