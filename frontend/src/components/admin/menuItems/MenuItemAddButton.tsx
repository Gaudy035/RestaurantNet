'use client'

import {useEmployee} from "@/lib/employee-context";
import {cn} from "@/lib/utils";
import {buttonVariants} from "@/components/ui/button";
import Link from "next/link";

export default function MenuItemAddButton(){
    const employee = useEmployee();
    const isAdmin = employee.employeeData?.isAdmin;

    if (isAdmin) {
        return (
            <Link
                href='/admin/menuitems/add'
                className={cn(
                    buttonVariants({ variant: 'default', size: 'default' }),
                )}
            >
                Add new menu item
            </Link>
        );
    }

    return null;
}