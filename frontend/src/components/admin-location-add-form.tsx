'use client';

import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Field, FieldGroup, FieldLabel } from '@/components/ui/field';
import { Input } from '@/components/ui/input';
import React from 'react';
import { adminApiFetch } from '@/lib/api';
import { useState } from 'react';
import { useRouter } from 'next/navigation';

export function AdminLocationAddForm({
  ...props
}: React.ComponentProps<typeof Card>) {
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();

  const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const formData = new FormData(e.currentTarget);
    let payload = Object.fromEntries(formData.entries());

    try {
      await adminApiFetch('/admin/locations', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      setError(null);
      alert('Location added');
      router.push('/admin/locations');
    } catch (err: any) {
      setError(err?.message ?? 'API error, try again later');
    }
  };

  return (
    <Card {...props}>
      <CardHeader>
        <CardTitle>Create a new location</CardTitle>
        <CardDescription>Enter new locations address below</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={onSubmit}>
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor='firstName'>City</FieldLabel>
              <Input
                id='city'
                name='city'
                type='text'
                placeholder='City'
                required
              />
            </Field>
            <Field>
              <FieldLabel htmlFor='address'>Address</FieldLabel>
              <Input
                id='address'
                name='address'
                type='text'
                placeholder='Address'
                required
              />
            </Field>
            {error ? <p className='text-destructive'>{error}</p> : null}
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
