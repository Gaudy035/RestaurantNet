'use client'

import Link from "next/link";
import {cn} from "@/lib/utils";
import {buttonVariants} from "@/components/ui/button";
import {useEmployee} from "@/lib/employee-context";

export default function CategoryAddButton () {
    const employee = useEmployee();
    const isAdmin = employee.employeeData?.isAdmin;

    if (isAdmin){
        return (
            <Link
                href='/admin/categories/add'
                className={cn(
                    buttonVariants({ variant: 'default', size: 'default' }),
                )}
            >
                Add new category
            </Link>
        )
    }
    return null;
}