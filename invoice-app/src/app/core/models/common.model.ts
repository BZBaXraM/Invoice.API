export interface ResponseModel {
  message: string;
  isSucceeded: boolean;
  isFailed: boolean;
  statusCode: number;
  errors?: string[] | null;
}

export interface ResponseModelData<T> extends ResponseModel {
  data: T | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
