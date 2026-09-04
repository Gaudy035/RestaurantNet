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

export function AdminEmployeeAddForm({
  ...props
}: React.ComponentProps<typeof Card>) {
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();

  const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const formData = new FormData(e.currentTarget);
    let payload = {
      ...Object.fromEntries(formData.entries()),
      email: formData.get('email')?.toString().trim().toLowerCase(),
      isAdmin: formData.get('isAdmin') === 'on',
    };

    try {
      await adminApiFetch('/admin/user/employees', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      setError(null);
      alert('Employee account created');
      router.push('/admin/employees');
    } catch (err: any) {
      setError(err?.message ?? 'API error, tru again later');
    }
  };

  return (
    <Card {...props}>
      <CardHeader>
        <CardTitle>Create an employee account</CardTitle>
        <CardDescription>Enter new employees information below</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={onSubmit}>
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor='firstName'>First name</FieldLabel>
              <Input
                id='firstName'
                name='firstName'
                type='text'
                placeholder='First name'
                required
              />
            </Field>
            <Field>
              <FieldLabel htmlFor='lastName'>Last name</FieldLabel>
              <Input
                id='lastName'
                name='lastName'
                type='text'
                placeholder='Last name'
                required
              />
            </Field>
            <Field>
              <FieldLabel htmlFor='email'>Email</FieldLabel>
              <Input
                id='email'
                type='email'
                name='email'
                placeholder='employee@example.com'
                required
              />
            </Field>
            <Field>
              <FieldLabel htmlFor='password'>Password</FieldLabel>
              <Input id='password' type='password' name='password' required />
            </Field>
            <Field>
              <div className='flex flex-row justify-start items-center gap-2'>
                <FieldLabel htmlFor='isAdmin'>Administrator access?</FieldLabel>
                <Input
                  id='isAdmin'
                  type='checkbox'
                  name='isAdmin'
                  className='size-4'
                />
              </div>
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
