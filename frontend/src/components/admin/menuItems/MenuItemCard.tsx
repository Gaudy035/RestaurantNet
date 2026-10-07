'use client'

import MenuItem from "@/interfaces/MenuItem";
import {useRouter} from "next/navigation";
import {Card, CardAction, CardDescription, CardHeader, CardTitle} from "@/components/ui/card";
import {Badge} from "@/components/ui/badge";
import {Button} from "@/components/ui/button";
import {useEmployee} from "@/lib/employee-context";

export default function MenuItemCard({ itemData }: { itemData: MenuItem }) {
    const router = useRouter();
    const employee = useEmployee();
    const isAdmin = employee.employeeData?.isAdmin;

    return (
        <Card
            className='group flex w-full px-2 py-5 cursor-pointer hover:bg-accent hover:text-accent-foreground transition'
            onClick={() => router.push(`/admin/menuitems/${itemData.itemId}`)}
        >
            <CardHeader className='flex flex-row justify-between items-center'>
                <div className='flex flex-col justify-center items-start gap-2'>
                    <CardTitle className='flex justify-center items-center flex-row gap-2'>
                        {itemData.name}
                        {isAdmin && <Badge>{itemData.isAvailable ? 'Available' : 'Unavailable'}</Badge>}
                    </CardTitle>
                    <CardDescription className='flex flex-row justify-center items-center gap-4'>
                        <p>Price: {(itemData.price.toFixed(2))}</p>
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