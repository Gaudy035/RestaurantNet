'use client';

import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from '@/components/ui/card';
import { Field, FieldGroup, FieldLabel } from '@/components/ui/field';
import React from 'react';
import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import EmployeeData from '@/interfaces/EmployeeData';
import LocationData from '@/interfaces/LocationData';
import Assignment from '@/interfaces/Assignment';
import Position from '@/types/position';
import { getErrorMessage } from '@/lib/api-error';

export function AdminEmployeeAssignForm({
  employeeId,
}: {
  employeeId: string;
}) {
  const [employeeData, setEmployeeData] = useState<EmployeeData | null>(null);
  const [employeeLoading, setEmployeeLoading] = useState<boolean>(false);
  const [employeeError, setEmployeeError] = useState<string | null>(null);

  const [locations, setLocations] = useState<LocationData[]>([]);
  const [locationsLoading, setLocationsLoading] = useState<boolean>(false);
  const [locationsError, setLocationsError] = useState<string | null>(null);

  const [position, setPosition] = useState<Position | null>(null);
  const [locationId, setLocationId] = useState<string | null>(null);

  const [formError, setFormError] = useState<string | null>(null);
  const router = useRouter();

  const positions: Position[] = [
    'Manager',
    'Chef',
    'Server',
    'Cashier',
    'Driver',
  ];

  //   Get Locations
  useEffect(() => {
    setLocationsLoading(true);
    setLocationsError(null);

    adminApiFetch<LocationData[]>('/admin/locations', { method: 'GET' })
      .then((res) => setLocations(res))
      .catch((e) => setLocationsError(getErrorMessage(e)))
      .finally(() => setLocationsLoading(false));
  }, []);

  // Get employee data
  useEffect(() => {
    setEmployeeLoading(true);
    setEmployeeError(null);

    adminApiFetch<EmployeeData>(`/admin/users/employees/${employeeId}`, {
      method: 'GET',
      cache: 'no-store',
    })
      .then((res) => setEmployeeData(res))
      .catch((e) => setEmployeeError(getErrorMessage(e)))
      .finally(() => setEmployeeLoading(false));
  }, [employeeId]);

  const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setFormError(null);

    if (!employeeId || !locationId || !position) {
      setFormError('Select location and position');
      return;
    }

    let payload: Assignment = {
      userId: employeeId,
      locationId: locationId!,
      position: position!,
    };

    try {
      await adminApiFetch('/admin/locations/employees', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      setFormError(null);
      alert('Employee assigned succesfully');
      router.push(`/admin/employees/${employeeId}`);
    } catch (err) {
      setFormError(getErrorMessage(err));
    }
  };

  if (locationsLoading || employeeLoading) {
    return <p>Loading...</p>;
  }

  if (employeeError || locationsError) {
    return (
      <div className='flex flex-col justify-center items-center text-destructive'>
        {employeeError ? <p>{employeeError}</p> : null}
        {locationsError ? <p>{locationsError}</p> : null}
      </div>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className='flex gap-2'>
          <span className='font-light'>Assign employee</span>
          <span>
            {employeeData?.firstName} {employeeData?.lastName}
          </span>
        </CardTitle>
        <CardDescription>Select location and positions</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={onSubmit}>
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor='locationId'>Location</FieldLabel>
              <select
                onChange={(e) => setLocationId(e.target.value)}
                className={
                  locationId ? 'text-foreground' : 'text-muted-foreground'
                }
                name='locationId'
                id='locationId'
                defaultValue=''
                required
              >
                <option value='' disabled>
                  Select location
                </option>
                {locations.map((l) => (
                  <option key={l.locationId} value={l.locationId}>
                    {l.city}, {l.address}
                  </option>
                ))}
              </select>
            </Field>
            <Field>
              <FieldLabel htmlFor='position'>Position</FieldLabel>
              <select
                onChange={(e) => setPosition(e.target.value as Position)}
                className={
                  position ? 'text-foreground' : 'text-muted-foreground'
                }
                id='position'
                name='position'
                defaultValue=''
                required
              >
                <option value='' disabled>
                  Select a position
                </option>
                {positions.map((p) => (
                  <option key={p} value={p}>
                    {p}
                  </option>
                ))}
              </select>
            </Field>
            {formError ? <p className='text-destructive'>{formError}</p> : null}
            <FieldGroup>
              <Field>
                <Button type='submit'>Submit</Button>
              </Field>
            </FieldGroup>
          </FieldGroup>
        </form>
      </CardContent>
    </Card>
  );
}
