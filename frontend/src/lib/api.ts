import { redirect } from 'next/navigation';

const isServer = typeof window === 'undefined';

const BASE_URL = isServer
  ? process.env.API_URL_SERVER
  : process.env.NEXT_PUBLIC_API_URL_CLIENT;

const parseResponseBody = async (response: Response) => {
  if (response.status === 204) {
    return null;
  }

  const text = await response.text();

  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
};

const createApiFetch = (surface: 'admin' | 'store') => {
  const refreshEndpoint =
    surface === 'admin' ? '/admin/auth/refresh' : '/auth/refresh';
  const loginEndpoint =
    surface === 'admin' ? '/admin/auth/login' : '/auth/login';
  const loginPath = surface === 'admin' ? '/admin/login' : '/login';

  let refreshPromise: Promise<boolean> | null = null;

  const refresh = (): Promise<boolean> => {
    if (!refreshPromise) {
      refreshPromise = fetch(`${BASE_URL}${refreshEndpoint}`, {
        method: 'POST',
        credentials: 'include',
      })
        .then((res) => res.ok)
        .catch(() => false)
        .finally(() => (refreshPromise = null));
    }
    return refreshPromise;
  };

  return async function apiFetch(endpoint: string, options: RequestInit = {}) {
    const defaultHeaders: Record<string, string> = {
      'Content-Type': 'application/json',
    };

    const response = await fetch(`${BASE_URL}${endpoint}`, {
      ...options,
      credentials: 'include',
      headers: { ...defaultHeaders, ...options.headers },
    });

    if (response.status === 401 && endpoint !== loginEndpoint) {
      if (endpoint === refreshEndpoint) {
        if (isServer) {
          redirect(loginPath);
        } else {
          window.location.href = loginPath;
        }
        throw new Error('Session expired');
      }

      const refreshed = await refresh();

      if (!refreshed) {
        if (isServer) {
          redirect(loginPath);
        } else {
          window.location.href = loginPath;
        }
        throw new Error('Session expired');
      }

      const retryResponse = await fetch(`${BASE_URL}${endpoint}`, {
        ...options,
        credentials: 'include',
        headers: { ...defaultHeaders, ...options.headers },
      });

      const retryData = await parseResponseBody(retryResponse);

      if (!retryResponse.ok) {
        throw new Error(retryData?.message || 'API request failed');
      }

      return retryData;
    }

    const data = await parseResponseBody(response);

    if (!response.ok) {
      throw new Error(data?.message || 'API request failed');
    }

    return data;
  };
};

export const adminApiFetch = createApiFetch('admin');
export const clientApiFetch = createApiFetch('store');
