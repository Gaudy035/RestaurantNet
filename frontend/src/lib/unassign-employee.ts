import Position from '@/types/Position';
import Assignment from '@/interfaces/Assignment';
import { adminApiFetch } from './api';

export default function unassignEmployee(
  employeeId: string,
  locationId: string,
  position: Position,
) {
  const payload: Assignment = {
    userId: employeeId,
    locationId: locationId,
    position: position,
  };

  return adminApiFetch('/admin/locations/employees', {
    method: 'DELETE',
    body: JSON.stringify(payload),
  });
}
