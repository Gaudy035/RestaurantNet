'use client';

import { adminApiFetch } from '@/lib/api';
import {
  Card,
  CardAction,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { useState, useEffect } from 'react';
import LocationEmployeeData from '@/interfaces/LocationEmployeeData';
import Position from '@/types/position';
import unassignEmployee from '@/lib/unassign-employee';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useRouter } from 'next/navigation';
import { getErrorMessage } from '@/lib/api-error';
import { toast } from 'sonner';

export default function LocationEmployees({
  locationId,
}: {
  locationId: string;
}) {
  const [employees, setEmployees] = useState<LocationEmployeeData[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const router = useRouter();

  useEffect(() => {
    adminApiFetch<LocationEmployeeData[]>(
      `/admin/locations/${locationId}/employees`,
      {
        method: 'GET',
        cache: 'no-store',
      },
    )
      .then((res) => setEmployees(res))
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, [locationId]);

  const handleUnassign = async (employeeId: string, position: Position) => {
    setError(null);

    try {
      await unassignEmployee(employeeId, locationId, position);
      setEmployees((prev) =>
        prev.filter(
          (e) => !(e.userId === employeeId && e.position === position),
        ),
      );
      toast.success('Employee unassigned successfully');
    } catch (e) {
      setError(getErrorMessage(e));
    }
  };

  return (
    <div className='flex justify-center flex-col items-center'>
      <Card className='flex w-full py-4 px-2'>
        {loading ? (
          'Loading employees...'
        ) : error ? (
          <p className='text-destructive'>{error}</p>
        ) : (
          <>
            <CardHeader className='flex flex-row justify-between items-center'>
              <CardTitle className='text-lg'>Employees</CardTitle>
              <CardAction>
                <Button
                  className='font-light'
                  variant='link'
                  onClick={() => router.push('/admin/employees')}
                >
                  Assign staff in employees page
                </Button>
              </CardAction>
            </CardHeader>
            <CardContent>
              {employees.length > 0 ? (
                <div className='divide-y'>
                  {employees.map((e) => (
                    <div
                      className='flex flex-row justify-between items-center w-full py-2'
                      key={`${e.userId}-${e.position}`}
                    >
                      <div className='flex flex-row justify-center items-center gap-4'>
                        <Button
                          variant='link'
                          onClick={() =>
                            router.push(`/admin/employees/${e.userId}`)
                          }
                        >
                          {e.firstName} {e.lastName}
                        </Button>
                        <Badge>{e.position}</Badge>
                      </div>
                      <Button
                        variant='link'
                        className='text-destructive'
                        onClick={() => {
                          if (
                            window.confirm(
                              `Unassign user ${e.firstName} ${e.lastName} (${e.position})`,
                            )
                          ) {
                            handleUnassign(e.userId, e.position);
                          }
                        }}
                      >
                        Unassign
                      </Button>
                    </div>
                  ))}
                </div>
              ) : (
                <div className='flex w-full justify-center items-center p-4'>
                  No assigned employees found
                </div>
              )}
            </CardContent>
          </>
        )}
      </Card>
    </div>
  );
}
