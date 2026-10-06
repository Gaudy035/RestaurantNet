import CategoryInfo from "@/components/admin/categories/details/CategoryInfo";

export default async function AdminCategoryDetailsPage({ params} : { params: Promise<{ id: string }> }){
    const { id } = await params;

    return (
        <div className='flex flex-1 flex-col m-4 gap-4'>
            <CategoryInfo categoryId={ id } />
        {/*  items, will get updated after creating item card  */}
        </div>
    );
}