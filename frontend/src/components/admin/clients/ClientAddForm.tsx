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

export function AdminClientAddForm({
  ...props
}: React.ComponentProps<typeof Card>) {
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();
  const phoneRegex = /^\+?[1-9]\d{1,14}$/;

  const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const formData = new FormData(e.currentTarget);
    let payload = Object.fromEntries(formData.entries());
    payload.email = payload.email.toString().trim().toLowerCase();

    const phoneNumber = payload.phoneNumber
      .toString()
      .trim()
      .replace(/[\s\-\(\)]/g, '');

    if (!phoneRegex.test(phoneNumber)) {
      setError('Invalid phone number');
      return;
    }

    payload.phoneNumber = phoneNumber;

    try {
      await adminApiFetch('/admin/users/clients', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      setError(null);
      alert('Client account created');
      router.push('/admin/clients');
    } catch (err: any) {
      setError(err?.message ?? 'API error, try again later');
    }
  };

  return (
    <Card {...props}>
      <CardHeader>
        <CardTitle>Create a client account</CardTitle>
        <CardDescription>Enter clients information below</CardDescription>
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
                placeholder='client@example.com'
                required
              />
            </Field>
            <Field>
              <FieldLabel htmlFor='password'>Password</FieldLabel>
              <Input id='password' type='password' name='password' required />
            </Field>
            <Field>
              <FieldLabel htmlFor='phoneNumber'>Phone number</FieldLabel>
              <Input
                id='phoneNumber'
                type='tel'
                name='phoneNumber'
                placeholder='XXXXXXXXX'
                required
              />
              <p className='text-destructive'>{error ? error : null}</p>
            </Field>
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
