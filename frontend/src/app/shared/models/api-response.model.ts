export interface ApiResponse<T> {
  error: boolean;
  message: string;
  code: number;
  data: T;
}