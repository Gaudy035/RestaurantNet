'use client';

import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from '@/components/ui/field';
import { Input } from '@/components/ui/input';
import React from 'react';
import { adminApiFetch } from '@/lib/api';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { getErrorMessage, isApiError } from '@/lib/api-error';
import { toast } from 'sonner';

export function AdminClientAddForm() {
  const [error, setError] = useState<string | null>(null);
  const [emailError, setEmailError] = useState<string | null>(null);
  const [phoneError, setPhoneError] = useState<string | null>(null);
  const router = useRouter();
  const phoneRegex = /^\+?[1-9]\d{1,14}$/;

  const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setEmailError(null);
    setPhoneError(null);

    const formData = new FormData(e.currentTarget);
    const payload = Object.fromEntries(formData.entries());
    payload.email = payload.email.toString().trim().toLowerCase();

    const phoneNumber = payload.phoneNumber
      .toString()
      .trim()
      .replace(/[\s\-\(\)]/g, '');

    if (!phoneRegex.test(phoneNumber)) {
      setPhoneError('Invalid phone number');
      return;
    }

    payload.phoneNumber = phoneNumber;

    try {
      await adminApiFetch('/admin/users/clients', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      setError(null);
      toast.success('Client account created');
      router.push('/admin/clients');
    } catch (err) {
      if (isApiError(err) && err.code === 'EmailAlreadyTaken') {
        setEmailError(err.message);
      } else {
        setError(getErrorMessage(err));
      }
    }
  };

  return (
    <Card>
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
                aria-invalid={emailError !== null}
                onChange={() => setEmailError(null)}
                required
              />
              {emailError ? <FieldError>{emailError}</FieldError> : null}
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
                aria-invalid={phoneError !== null}
                onChange={() => setPhoneError(null)}
                required
              />
              {phoneError ? <FieldError>{phoneError}</FieldError> : null}
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
