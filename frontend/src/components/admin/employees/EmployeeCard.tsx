'use client';

import EmployeeData from '@/interfaces/EmployeeData';
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from '../../ui/card';
import { Badge } from '../../ui/badge';
import { Button } from '../../ui/button';
import { useRouter } from 'next/navigation';

export default function EmployeeCard({
  employeeData,
}: {
  employeeData: EmployeeData;
}) {
  const router = useRouter();

  return (
    <Card
      className='group flex w-full px-4 py-6 cursor-pointer hover:bg-accent hover:text-accent-foreground transition'
      onClick={() => router.push(`/admin/employees/${employeeData.userId}`)}
    >
      <CardHeader className='flex flex-row justify-between items-center'>
        <div className='flex flex-col justify-center items-start gap-2'>
          <CardTitle className='flex justify-center items-center flex-row gap-2'>
            {employeeData.firstName} {employeeData.lastName}
            {employeeData.isAdmin ? <Badge>Administrator</Badge> : null}
          </CardTitle>
          <CardDescription className='flex flex-row justify-center items-center gap-4'>
            <p>Email: {employeeData.email}</p>
          </CardDescription>
        </div>

        <CardAction>
          <Button
            variant='link'
            size={'lg'}
            className='pointer-events-none group-hover:underline'
          >
            Details
          </Button>
        </CardAction>
      </CardHeader>
    </Card>
  );
}
