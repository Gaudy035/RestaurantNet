import React from 'react';
import ModeToggle from '@/components/ui/mode-toggle';
import AdminLogoutButton from '@/components/ui/admin-logout-button';

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className='flex min-h-screen'>
      <ModeToggle></ModeToggle>
      <AdminLogoutButton></AdminLogoutButton>
      <main className='flex flex-1 justify-center items-center'>
        {children}
      </main>
    </div>
  );
}
