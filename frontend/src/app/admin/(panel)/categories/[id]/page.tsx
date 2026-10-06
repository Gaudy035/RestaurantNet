import CategoryInfo from "@/components/admin/categories/details/CategoryInfo";
import CategoryItems from "@/components/admin/categories/details/CategoryItems";

export default async function AdminCategoryDetailsPage({ params} : { params: Promise<{ id: string }> }){
    const { id } = await params;

    return (
        <div className='flex flex-1 flex-col m-4 gap-4'>
            <CategoryInfo categoryId={ id } />
            <CategoryItems categoryId={ id } />
        </div>
    );
}