'use client';

import ModeToggle from '../ui/mode-toggle';
import { adminApiFetch } from '@/lib/api';
import { useRouter } from 'next/navigation';

import { cn } from '@/lib/utils';
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
import React, { useState } from 'react';
import { getErrorMessage } from '@/lib/api-error';

export function AdminLoginForm() {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [emailError, setEmailError] = useState<string | null>(null);

  const handleSubmit = async (event: React.SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setEmailError(null);

    const formData = new FormData(event.currentTarget);
    const payload = Object.fromEntries(formData.entries());

    try {
      await adminApiFetch('/admin/auth/login', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      router.push('/admin/');
      router.refresh();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  return (
    <div className={cn('flex flex-col gap-6')}>
      <Card>
        <CardHeader>
          <div className='flex flex-row items-center justify-between'>
            <CardTitle>Admin panel</CardTitle>
            <ModeToggle></ModeToggle>
          </div>
          <CardDescription className={error ? 'text-destructive' : ''}>
            {error === null
              ? 'Enter your email below to login to your account'
              : error}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit}>
            <FieldGroup>
              <Field>
                <FieldLabel htmlFor='email'>Email</FieldLabel>
                <Input
                  id='email'
                  type='email'
                  name='email'
                  placeholder='name@restaurant.com'
                  aria-invalid={emailError !== null}
                  onChange={() => setEmailError(null)}
                  required
                />
                {emailError ? <FieldError>{emailError}</FieldError> : null}
              </Field>
              <Field>
                <div className='flex items-center'>
                  <FieldLabel htmlFor='password'>Password</FieldLabel>
                </div>
                <Input id='password' type='password' name='password' required />
              </Field>
              <Field>
                <Button type='submit'>Login</Button>
              </Field>
            </FieldGroup>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
