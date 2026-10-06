import AdminCategoryAddForm from "@/components/admin/categories/CategoryAddForm";

export default function AdminCategoryAddPage(){
    return (
        <div className='flex flex-1 justify-center items-center'>
            <div className='w-full max-w-sm'>
                <AdminCategoryAddForm />
            </div>
        </div>
    );
}