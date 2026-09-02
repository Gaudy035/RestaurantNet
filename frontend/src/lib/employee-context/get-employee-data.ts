import EmployeeData from '@/interfaces/EmployeeData';
import { adminApiFetch } from '../api';

export default async function getEmployeeData(): Promise<EmployeeData | null> {
  try {
    const employeeData = await adminApiFetch('/admin/auth/me', {
      method: 'GET',
      cache: 'no-store',
    });

    return employeeData;
  } catch {
    return null;
  }
}
