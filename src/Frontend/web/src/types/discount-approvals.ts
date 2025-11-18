// TypeScript types for Discount Approvals matching backend DTOs

export type ApprovalStatus = "Pending" | "Approved" | "Rejected";
export type ApprovalLevel = "Manager" | "Admin";

export interface DiscountApproval {
  approvalId: string;
  quotationId: string;
  quotationNumber: string;
  clientId: string;
  clientName: string;
  requestedByUserId: string;
  requestedByUserName: string;
  approverUserId?: string;
  approverUserName?: string;
  status: ApprovalStatus;
  requestDate: string;
  approvalDate?: string;
  rejectionDate?: string;
  currentDiscountPercentage: number;
  threshold: number;
  approvalLevel: ApprovalLevel;
  reason: string;
  comments?: string;
  escalatedToAdmin: boolean;
  createdAt: string;
  updatedAt: string;
  isPending: boolean;
  isApproved: boolean;
  isRejected: boolean;
}

export interface CreateDiscountApprovalRequest {
  quotationId: string;
  discountPercentage: number;
  reason: string;
  comments?: string;
}

export interface ApproveDiscountApprovalRequest {
  reason: string;
  comments?: string;
}

export interface RejectDiscountApprovalRequest {
  reason: string;
  comments?: string;
}

export interface ResubmitDiscountApprovalRequest {
  reason: string;
  comments?: string;
}

export interface BulkApproveRequest {
  approvalIds: string[];
  reason: string;
  comments?: string;
}

export interface ApprovalTimeline {
  approvalId: string;
  quotationId: string;
  eventType: "Requested" | "Approved" | "Rejected" | "Escalated" | "Resubmitted";
  status: ApprovalStatus;
  previousStatus?: ApprovalStatus;
  userId: string;
  userName: string;
  userRole: string;
  reason: string;
  comments?: string;
  timestamp: string;
}

export interface ApprovalMetrics {
  pendingCount: number;
  approvedCount: number;
  rejectedCount: number;
  totalCount: number;
  averageApprovalTime?: string; // ISO duration string
  rejectionRate: number;
  averageDiscountPercentage: number;
  escalationCount: number;
  dateFrom?: string;
  dateTo?: string;
}

export interface PagedApprovalsResult {
  success: boolean;
  data: {
    data: DiscountApproval[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
  };
}

