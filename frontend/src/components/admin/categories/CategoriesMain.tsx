'use client'

import {useEffect, useState} from "react";
import Category from "@/interfaces/Category";
import {adminApiFetch} from "@/lib/api";
import {getErrorMessage} from "@/lib/api-error";
import {Spinner} from "@/components/ui/spinner";
import CategoryCard from "@/components/admin/categories/CategoryCard";

export default function AdminCategoriesMain() {
    const [categories, setCategories] = useState<Category[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        adminApiFetch<Category[]>('/admin/categories', {method: 'GET', cache: "no-store"})
            .then((data) => {
                setError(null);
                setCategories(data);
            })
            .catch((e) => setError(getErrorMessage(e)))
            .finally(() => setLoading(false));
    }, [categories]);

    return (
        <div className='flex justify-center flex-col items-center'>
            <p className='text-destructive'>{error ? error : null}</p>
            {loading ? (
                <Spinner className='size-8' />
            ) : categories.length > 0 ? (
                <div className='flex flex-col m-8 w-full justify-center items-center gap-4'>
                    {categories.map((c) => (
                        <CategoryCard
                            key={c.categoryId}
                            categoryData={c}
                        />
                    ))}
                </div>
            ) : (
                <p>No categories found</p>
            )}
        </div>
    );
}