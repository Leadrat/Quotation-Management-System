const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export const validateEmail = (value: string) => EMAIL_REGEX.test(value.trim());

