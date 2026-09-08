'use client';

import { useEmployee } from '@/lib/employee-context';
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';

export default function AdminHome() {
  const employeeContext = useEmployee();

  return (
    <div className='flex flex-1 max-h-2/3 p-12 justify-center items-center'>
      {employeeContext.loading ? (
        <p>Loading...</p>
      ) : (
        <Card className='w-full max-w-sm'>
          <CardHeader>
            <CardTitle>
              Welcome back {employeeContext.employeeData?.firstName}!
            </CardTitle>
            <CardDescription>
              Select a page from sidebar to get started
            </CardDescription>
          </CardHeader>
        </Card>
      )}
    </div>
  );
}
