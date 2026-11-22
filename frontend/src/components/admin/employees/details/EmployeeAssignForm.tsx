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
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { getErrorMessage } from '@/lib/api-error';
import { toast } from 'sonner';
import { Spinner } from '@/components/ui/spinner';

export function AdminEmployeeAssignForm({
  employeeId,
}: {
  employeeId: string;
}) {
  const [employeeData, setEmployeeData] = useState<EmployeeData | null>(null);
  const [employeeLoading, setEmployeeLoading] = useState<boolean>(true);
  const [employeeError, setEmployeeError] = useState<string | null>(null);

  const [locations, setLocations] = useState<LocationData[]>([]);
  const [locationsLoading, setLocationsLoading] = useState<boolean>(true);
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
    adminApiFetch<LocationData[]>('/admin/locations', { method: 'GET' })
      .then((res) => {
        setLocationsError(null);
        setLocations(res);
      })
      .catch((e) => setLocationsError(getErrorMessage(e)))
      .finally(() => setLocationsLoading(false));
  }, []);

  // Get employee data
  useEffect(() => {
    adminApiFetch<EmployeeData>(`/admin/users/employees/${employeeId}`, {
      method: 'GET',
      cache: 'no-store',
    })
      .then((res) => {
        setEmployeeError(null);
        setEmployeeData(res);
      })
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

    const payload: Assignment = {
      userId: Number(employeeId),
      locationId: Number(locationId),
      position: position!,
    };

    try {
      await adminApiFetch('/admin/locations/employees', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      setFormError(null);
      toast.success('Employee assigned successfully');
      router.push(`/admin/employees/${employeeId}`);
    } catch (err) {
      setFormError(getErrorMessage(err));
    }
  };

  if (locationsLoading || employeeLoading) {
    return <Spinner className='size-8' />;
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
              <Select
                items={locations.map((l) => ({
                  value: String(l.locationId),
                  label: `${l.city}, ${l.address}`,
                }))}
                value={locationId}
                onValueChange={(value) => setLocationId(value)}
              >
                <SelectTrigger id='locationId' className='w-full'>
                  <SelectValue placeholder='Select location' />
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {locations.map((l) => (
                      <SelectItem
                        key={l.locationId}
                        value={String(l.locationId)}
                      >
                        {l.city}, {l.address}
                      </SelectItem>
                    ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
            </Field>
            <Field>
              <FieldLabel htmlFor='position'>Position</FieldLabel>
              <Select
                items={positions.map((p) => ({ value: p, label: p }))}
                value={position}
                onValueChange={(value) =>
                  value && setPosition(value as Position)
                }
              >
                <SelectTrigger id='position' className='w-full'>
                  <SelectValue placeholder='Select a position' />
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {positions.map((p) => (
                      <SelectItem key={p} value={p}>
                        {p}
                      </SelectItem>
                    ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
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
