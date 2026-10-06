import AddCategoryButton from "@/components/admin/categories/CategoryAddButton";
import AdminCategoriesMain from "@/components/admin/categories/CategoriesMain";

export default function AdminCategoriesPage(){
    return (
        <div className='flex flex-1 flex-col m-4'>
            <div className='flex justify-between items-center'>
                <h1 className='text-2xl'>Categories</h1>
                <div className='flex justify-end items-center gap-2'>
                    <AddCategoryButton />
                </div>
            </div>
            <AdminCategoriesMain/>
        </div>
    )
}