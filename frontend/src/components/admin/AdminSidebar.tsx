'use client';

import * as React from 'react';

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from '@/components/ui/sidebar';
import { usePathname } from 'next/navigation';
import { useEmployee } from '@/lib/employee-context';
import Link from 'next/link';

const data = {
  navMain: [
    {
      title: 'Pages',
      url: '#',
      items: [
        {
          title: 'WIP',
          url: '/admin/wip',
        },
        {
          title: 'Clients',
          url: '/admin/clients',
        },
        {
          title: 'Employees',
          url: '/admin/employees',
          adminOnly: true,
        },
        {
          title: 'Locations',
          url: '/admin/locations',
          adminOnly: true,
        },
      ],
    },
  ],
};

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const pathname = usePathname();
  const employeeData = useEmployee();
  const isAdmin = employeeData.employeeData?.isAdmin;

  return (
    <Sidebar {...props}>
      <SidebarHeader>
        <Link href='/admin' className='flex justify-start items-center px-2'>
          <p className='font-semibold text-xl'>Admin panel</p>
        </Link>
      </SidebarHeader>
      <SidebarContent>
        {data.navMain.map((item) => (
          <SidebarGroup key={item.title}>
            <SidebarGroupLabel>{item.title}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {item.items
                  .filter((item) => !item.adminOnly || isAdmin)
                  .map((item) => {
                    const isActive = item.url === pathname;
                    return (
                      <SidebarMenuItem key={item.title}>
                        <SidebarMenuButton
                          isActive={isActive}
                          render={<a href={item.url} />}
                        >
                          {item.title}
                        </SidebarMenuButton>
                      </SidebarMenuItem>
                    );
                  })}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        ))}
      </SidebarContent>
      <SidebarRail />
    </Sidebar>
  );
}
