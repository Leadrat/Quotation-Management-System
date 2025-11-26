// Simple toast implementation
export const toast = {
  success: (message: string) => {
    console.log('✅ Success:', message);
    // In a real implementation, you would show a toast notification
    // For now, we'll just log to console
  },
  error: (message: string) => {
    console.error('❌ Error:', message);
    // In a real implementation, you would show an error toast
  },
  info: (message: string) => {
    console.log('ℹ️ Info:', message);
    // In a real implementation, you would show an info toast
  }
};