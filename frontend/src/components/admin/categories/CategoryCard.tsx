'use client';

import {
    Card,
    CardAction,
    CardHeader,
    CardTitle,
} from '../../ui/card';
import { Button } from '../../ui/button';
import { useRouter } from 'next/navigation';
import Category from "@/interfaces/Category";

export default function CategoryCard({ categoryData }: { categoryData: Category }) {
    const router = useRouter();

    return (
        <Card
            className='group flex w-full px-2 py-5 cursor-pointer hover:bg-accent hover:text-accent-foreground transition'
            onClick={() => router.push(`/admin/categories/${categoryData.categoryId}`)}
        >
            <CardHeader className='flex flex-row justify-between items-center'>
                <div className='flex flex-col justify-center items-start gap-2'>
                    <CardTitle className='flex justify-center items-center flex-row gap-2'>
                        { categoryData.categoryName }
                    </CardTitle>
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
