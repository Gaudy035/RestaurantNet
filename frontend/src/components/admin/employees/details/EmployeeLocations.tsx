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
import EmployeeLocationData from '@/interfaces/EmployeeLocationData';
import Position from '@/types/Position';
import unassignEmployee from '@/lib/unassign-employee';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useRouter } from 'next/navigation';

export default function EmployeeLocations({
  employeeId,
}: {
  employeeId: string;
}) {
  const [locations, setLocations] = useState<EmployeeLocationData[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const router = useRouter();

  useEffect(() => {
    setLoading(true);

    adminApiFetch(`/admin/users/employees/${employeeId}/locations`, {
      method: 'GET',
      cache: 'no-store',
    })
      .then((res) => setLocations(res))
      .catch((e: any) => setError(e?.message))
      .finally(() => setLoading(false));
  }, [employeeId]);

  const handleUnassign = async (locationId: string, position: Position) => {
    setError(null);

    try {
      await unassignEmployee(employeeId, locationId, position);
      setLocations((prev) => prev.filter((l) => l.locationId != locationId));
    } catch (e: any) {
      setError(e?.message);
      console.log(e?.message);
    }
  };

  return (
    <div className='flex justify-center flex-col items-center'>
      <Card className='flex w-full p-4'>
        {loading ? (
          'Loading locations...'
        ) : (
          <>
            <CardHeader className='flex flex-row justify-between items-center'>
              <CardTitle className='text-lg'>Locations</CardTitle>
              <CardAction>
                <Button
                  variant='default'
                  //   Link to assign form
                >
                  Assign to location
                </Button>
              </CardAction>
            </CardHeader>
            <CardContent>
              {locations.length > 0 ? (
                <div className='divide-y'>
                  {locations.map((l) => (
                    <div
                      className='flex flex-row justify-between items-center w-full py-2'
                      key={l.locationId}
                    >
                      <div className='flex flex-row justify-center items-center gap-4'>
                        <Button
                          variant='link'
                          onClick={() =>
                            router.push(`/admin/locations/${l.locationId}`)
                          }
                        >
                          {l.city}, {l.address}
                        </Button>
                        <Badge>{l.position}</Badge>
                      </div>
                      <Button
                        variant='link'
                        className='text-destructive'
                        onClick={() => {
                          if (
                            window.confirm(
                              `Unassign from location ${l.city}, ${l.address} (${l.position})`,
                            )
                          ) {
                            handleUnassign(l.locationId, l.position);
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
                  No location assignments found
                </div>
              )}
            </CardContent>
          </>
        )}
      </Card>
    </div>
  );
}
