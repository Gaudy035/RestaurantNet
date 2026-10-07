'use client'

import {useEffect, useState} from "react";
import Category from "@/interfaces/Category";
import {adminApiFetch} from "@/lib/api";
import {toast} from "sonner";
import {getErrorMessage, isApiError} from "@/lib/api-error";
import {Spinner} from "@/components/ui/spinner";
import {Card, CardAction,CardHeader, CardTitle} from "@/components/ui/card";
import ConfirmDialog from "@/components/admin/AdminConfirmDialog";
import {Button} from "@/components/ui/button";
import {useEmployee} from "@/lib/employee-context";
import {useRouter} from "next/navigation";

export default function CategoryInfo({ categoryId }: { categoryId: string }) {
    const router = useRouter();
    const [categoryInfo, setCategoryInfo] = useState<Category>();
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const employee = useEmployee();
    const isAdmin = employee.employeeData?.isAdmin;

    useEffect(() => {
        adminApiFetch<Category>(`/admin/categories/${categoryId}`, {
            method: 'GET',
            cache: 'no-store',
        })
            .then((data) => setCategoryInfo(data))
            .catch((error) => setError(error))
            .finally(() => setLoading(false));
    }, [categoryId]);

    const deleteCategory = async () => {
        setError(null);
        try {
            await adminApiFetch(`/admin/categories/${categoryId}`, {
                method: 'DELETE',
            });
            toast.success(`Category deleted successfully.`);
            router.push("/admin/categories/");
        } catch (error){
            if (isApiError(error)) {
                toast.error(getErrorMessage(error));
            } else {
                toast.error('Could not delete category.');
            }
        }
    }

    return (
        <div className='flex justify-center flex-col items-center'>
            <p className='text-destructive'>{error ? error : null}</p>
            {loading ? (
                <Spinner className='size-8' />
            ) : categoryInfo ? (
                <Card className='flex w-full py-6 px-3'>
                    <CardHeader className='flex flex-row justify-between items-center'>
                        <div className='flex flex-col justify-center items-start gap-2'>
                            <CardTitle className='flex justify-center items-center flex-row gap-4 text-2xl font-semibold'>
                                {categoryInfo.categoryName}
                            </CardTitle>
                        </div>
                        {isAdmin ?
                            <CardAction>
                                <ConfirmDialog
                                    trigger={
                                        <Button
                                            variant={'destructive'}
                                            className='text-lg p-4'
                                            size={'lg'}
                                        >
                                            {'Delete'}
                                        </Button>
                                    }
                                    variant='destructive'
                                    title='Delete employee'
                                    description={`Delete category: ${categoryInfo.categoryName}(ID: ${categoryInfo.categoryId})?`}
                                    onConfirm={() => deleteCategory()}
                                />
                            </CardAction>
                        : null }
                    </CardHeader>
                </Card>
            ) : null}
        </div>
    );
}