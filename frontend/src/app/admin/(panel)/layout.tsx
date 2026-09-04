import React from 'react';
import ModeToggle from '@/components/ui/mode-toggle';
import AdminLogoutButton from '@/components/ui/admin-logout-button';
import { AppSidebar } from '@/components/app-sidebar';
import { Separator } from '@/components/ui/separator';
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from '@/components/ui/sidebar';
import { EmployeeProvider } from '@/lib/employee-context';

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <EmployeeProvider>
      <SidebarProvider>
        <AppSidebar />
        <SidebarInset>
          <header className='flex h-16 shrink-0 items-center justify-between gap-2 border-b px-4'>
            <SidebarTrigger className='-ml-1' />
            <Separator
              orientation='vertical'
              className='mr-2 data-vertical:h-4 data-vertical:self-auto'
            />
            <div className='flex items-center justify-center gap-2'>
              <ModeToggle />
              <AdminLogoutButton />
            </div>
          </header>
          <main className='flex flex-1 flex-col'>{children}</main>
        </SidebarInset>
      </SidebarProvider>
    </EmployeeProvider>
  );
}
