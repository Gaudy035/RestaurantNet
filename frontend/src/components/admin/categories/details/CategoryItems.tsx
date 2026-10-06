'use client'

import {useEffect, useState} from "react";
import MenuItem from "@/interfaces/MenuItem";
import {adminApiFetch} from "@/lib/api";
import {getErrorMessage} from "@/lib/api-error";
import {Card, CardContent, CardHeader, CardTitle} from "@/components/ui/card";
import {Spinner} from "@/components/ui/spinner";
import {Button} from "@/components/ui/button";
import {Badge} from "@/components/ui/badge";
import {useRouter} from "next/navigation";
import {useEmployee} from "@/lib/employee-context";

export default function CategoryItems({ categoryId }: { categoryId: string }) {
    const [items, setItems] = useState<MenuItem[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const router = useRouter();

    const employee = useEmployee();
    const isAdmin = employee.employeeData?.isAdmin;

    useEffect(() => {
        adminApiFetch<MenuItem[]>(`/admin/categories/${categoryId}/items`, {
            method: 'GET',
            cache: 'no-store'
        })
            .then((data) => setItems(data))
            .catch((error) => setError(getErrorMessage(error)))
            .finally(() => setLoading(false))
    }, [categoryId]);

    return (
        <div className='flex justify-center flex-col items-center'>
            <Card className='flex w-full py-4 px-2'>
                {loading ? (
                    <Spinner className='size-8' />
                ) : error ? (
                    <p className='text-destructive'>{error}</p>
                ) : (
                    <>
                        <CardHeader className='flex flex-row justify-between items-center'>
                            <CardTitle className='text-lg'>Menu items</CardTitle>
                        </CardHeader>
                        <CardContent>
                            {items.length > 0 ? (
                                <div className='divide-y'>
                                    {items.map((mi) => (
                                        <div
                                            className='flex flex-row justify-between items-center w-full py-1'
                                            key={mi.itemId}
                                        >
                                            <div className='flex flex-row justify-center items-center gap-4'>
                                                <Button
                                                    variant='link'
                                                    onClick={() =>
                                                        router.push(`/admin/menuitems/${mi.itemId}`)
                                                    }
                                                >
                                                    {mi.name}
                                                </Button>
                                                {isAdmin && <Badge>{mi.isAvailable ? 'Available' : 'Unavailable'}</Badge>}
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <div className='flex w-full justify-center items-center p-4'>
                                    No menu items found
                                </div>
                            )}
                        </CardContent>
                    </>
                )}
            </Card>
        </div>
    );
}