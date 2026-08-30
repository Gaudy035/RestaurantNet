'use client';

import { adminApiFetch } from '@/lib/api';
import { Button } from './button';
import { useRouter } from 'next/navigation';

export default function AdminLogoutButton() {
  const router = useRouter();

  const clickHandle = async () => {
    await adminApiFetch('/admin/auth/logout', { method: 'POST' });

    router.push('/admin/login');
    router.refresh();
  };
  return <Button onClick={clickHandle}>Log out</Button>;
}
