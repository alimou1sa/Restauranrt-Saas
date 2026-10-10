import type { ProductListResponse, ProductDetailsResponse, CreateProductRequest, UpdateProductRequest, UpdateProductAvailabilityRequest, MenuResponse, CategoryResponse, CreateMenuRequest, CreateCategoryRequest } from '../types/api';
import { http } from './client';
export const productsApi = {
 listByBranch: (branchId:number) => http.get<ProductListResponse[]>(`/Products/branches/${branchId}`).then(r=>r.data),
 get: (id:number) => http.get<ProductDetailsResponse>(`/Products/${id}`).then(r=>r.data),
 create: (categoryId:number,body:CreateProductRequest) => http.post<ProductDetailsResponse>(`/Products/categories/${categoryId}`,body).then(r=>r.data),
 update: (id:number,body:UpdateProductRequest) => http.put<ProductDetailsResponse>(`/Products/${id}`,body).then(r=>r.data),
 setAvailability: (id:number,body:UpdateProductAvailabilityRequest) => http.patch<ProductDetailsResponse>(`/Products/${id}/availability`,body).then(r=>r.data),
 remove: (id:number) => http.delete<void>(`/Products/${id}`),
 menus: (branchId:number) => http.get<MenuResponse[]>(`/Menus/branches/${branchId}`).then(r=>r.data),
 categories: (menuId:number) => http.get<CategoryResponse[]>(`/Categories/menus/${menuId}`).then(r=>r.data),
 createMenu: (branchId:number,body:CreateMenuRequest) => http.post<MenuResponse>(`/Menus/branches/${branchId}`,body).then(r=>r.data),
 createCategory: (menuId:number,body:CreateCategoryRequest) => http.post<CategoryResponse>(`/Categories/menus/${menuId}`,body).then(r=>r.data),
 updateMenu: (id:number,body:CreateMenuRequest & {isPublished:boolean;isActive:boolean}) => http.put<MenuResponse>(`/Menus/${id}`,body).then(r=>r.data),
 removeMenu: (id:number) => http.delete<void>(`/Menus/${id}`),
 updateCategory: (id:number,body:CreateCategoryRequest & {isActive:boolean}) => http.put<CategoryResponse>(`/Categories/${id}`,body).then(r=>r.data),
 removeCategory: (id:number) => http.delete<void>(`/Categories/${id}`),
};
