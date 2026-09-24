import BackendErrorCode from '@/types/backend-error-code';
import ProblemDetails from '@/types/problem-details';

type ApiErrorInit = {
  status: number;
  message: string;
  code?: BackendErrorCode | null;
  title?: string | null;
  detail?: string | null;
  traceId?: string | null;
};

export class ApiError extends Error {
  readonly status: number;
  readonly code: BackendErrorCode | null;
  readonly title: string | null;
  readonly detail: string | null;
  readonly traceId: string | null;

  constructor(init: ApiErrorInit, options?: ErrorOptions) {
    super(init.message, options);
    this.name = 'ApiError';
    this.status = init.status;
    this.code = init.code ?? null;
    this.title = init.title ?? null;
    this.detail = init.detail ?? null;
    this.traceId = init.traceId ?? null;
  }

  static fromResponse(response: Response, body: unknown): ApiError {
    if (typeof body === 'object' && body !== null) {
      const problem = body as ProblemDetails;
      const title = typeof problem.title === 'string' ? problem.title : null;
      const detail = typeof problem.detail === 'string' ? problem.detail : null;

      return new ApiError({
        status: response.status,
        message: detail ?? title ?? (response.statusText || 'Request failed'),
        code: title as BackendErrorCode | null,
        title: title,
        detail: detail,
        traceId: problem.traceId ?? null,
      });
    }

    return new ApiError({
      status: response.status,
      message: response.statusText || 'Request failed',
    });
  }
}

export const isApiError = (e: unknown): e is ApiError => e instanceof ApiError;

export const getErrorMessage = (
  e: unknown,
  fallback = 'Something went wrong, try again later',
): string => {
  if (isApiError(e)) {
    return e.detail ?? e.message ?? fallback;
  }
  if (e instanceof Error && e.message) {
    return e.message;
  }
  return fallback;
};
