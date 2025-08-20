import React, { useEffect, useState } from "react";

function AdminCategories() {
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const apiUrl = import.meta.env.VITE_API_URL;
  const [subCatInputs, setSubCatInputs] = useState({});
  const [editingCategoryId, setEditingCategoryId] = useState(null);
  const [editCategoryName, setEditCategoryName] = useState("");
  const [editCategoryImage, setEditCategoryImage] = useState(null);
  const [editingSubCategoryId, setEditingSubCategoryId] = useState(null);
  const [editSubCategoryName, setEditSubCategoryName] = useState("");
  const [editSubCategoryImage, setEditSubCategoryImage] = useState(null);

  useEffect(() => {
    fetchCategories();
  }, []);

  const fetchCategories = async () => {
    setLoading(true);
    const token = localStorage.getItem("token");
    const res = await fetch(`${apiUrl}/categories`, {
      headers: { "Authorization": `Bearer ${token}` }
    });
    if (res.ok) {
      const data = await res.json();
      setCategories(data);
    }
    setLoading(false);
  };

    const handleDeleteCategory = async (categoryId) => {
    const token = localStorage.getItem("token");
    if (!window.confirm("Delete this category and all its subcategories?")) return;
    const res = await fetch(`${apiUrl}/categories/${categoryId}`, {
      method: "DELETE",
      headers: { "Authorization": `Bearer ${token}` }
    });
    if (res.ok) {
      setCategories((prev) => prev.filter((cat) => cat.categoryId !== categoryId));
    }
  };

  const handleDeleteSubCategory = async (subCategoryId, categoryId) => {
    const token = localStorage.getItem("token");
    if (!window.confirm("Delete this subcategory?")) return;
    const res = await fetch(`${apiUrl}/subcategories/${subCategoryId}`, {
      method: "DELETE",
      headers: { "Authorization": `Bearer ${token}` }
    });
    if (res.ok) {
      setCategories((prev) =>
        prev.map((cat) =>
          cat.categoryId === categoryId
            ? {
                ...cat,
                subCategories: cat.subCategories.filter(
                  (sub) => sub.subCategoryId !== subCategoryId
                ),
              }
            : cat
        )
      );
    }
  };

return (
    <div>
      <form
        onSubmit={async (e) => {
          e.preventDefault();
          const formData = new FormData(e.target);
          const token = localStorage.getItem("token");
          const res = await fetch(`${apiUrl}/categories`, {
            method: "POST",
            headers: {
              Authorization: `Bearer ${token}`,
            },
            body: formData,
          });
          if (res.ok) {
            e.target.reset();
            fetchCategories();
          }
        }}
        style={{
          marginBottom: "2rem",
          background: "#222",
          borderRadius: 8,
          padding: 16,
          color: "#fff",
          display: "flex",
          alignItems: "center",
          gap: 12,
        }}
      >
        <input
          type="text"
          name="CategoryName"
          placeholder="New category name"
          required
          style={{
            border: "none",
            background: "transparent",
            color: "white",
            outline: "none",
            fontSize: "1em",
            flex: 1,
          }}
        />
        <input
          type="file"
          name="Image"
          accept="image/*"
          style={{ color: "white" }}
        />
        <button
          type="submit"
          style={{
            background: "green",
            color: "white",
            border: "none",
            padding: "6px 16px",
            borderRadius: "4px",
            cursor: "pointer",
          }}
        >
          Create Category
        </button>
      </form>
      <h2>Categories & Subcategories</h2>
      {loading ? (
        <div>Loading...</div>
      ) : (
        <div>
          {categories.map((cat) => (
            <ul
              key={cat.categoryId}
              style={{
                marginBottom: "2rem",
                background: "#222",
                borderRadius: 8,
                padding: 16,
              }}
            >
              <li
                style={{
                  fontWeight: "bold",
                  fontSize: "1.1em",
                  color: "#fff",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                {editingCategoryId === cat.categoryId ? (
                  <form
                    onSubmit={async (e) => {
                      e.preventDefault();
                      const formData = new FormData();
                      formData.append("CategoryName", editCategoryName);
                      if (editCategoryImage) {
                        formData.append("Image", editCategoryImage);
                      }
                      const token = localStorage.getItem("token");
                      const res = await fetch(`${apiUrl}/categories/${cat.categoryId}`, {
                        method: "PUT",
                        headers: { Authorization: `Bearer ${token}` },
                        body: formData,
                      });
                      if (res.ok) {
                        setEditingCategoryId(null);
                        setEditCategoryName("");
                        setEditCategoryImage(null);
                        fetchCategories();
                      }
                    }}
                    style={{ display: "flex", alignItems: "center", gap: 8, flex: 1 }}
                  >
                    <input
                      type="text"
                      value={editCategoryName}
                      onChange={(e) => setEditCategoryName(e.target.value)}
                      required
                      style={{
                        border: "none",
                        background: "transparent",
                        color: "white",
                        outline: "none",
                        fontSize: "1em",
                        flex: 1,
                      }}
                    />
                    <input
                      type="file"
                      accept="image/*"
                      onChange={(e) => setEditCategoryImage(e.target.files[0])}
                      style={{ color: "white" }}
                    />
                    <button
                      type="submit"
                      style={{
                        background: "green",
                        color: "white",
                        border: "none",
                        padding: "4px 12px",
                        borderRadius: "4px",
                        cursor: "pointer",
                      }}
                    >
                      Save
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setEditingCategoryId(null);
                        setEditCategoryName("");
                        setEditCategoryImage(null);
                      }}
                      style={{
                        background: "#555",
                        color: "white",
                        border: "none",
                        padding: "4px 12px",
                        borderRadius: "4px",
                        cursor: "pointer",
                      }}
                    >
                      Cancel
                    </button>
                  </form>
                ) : (
                  <>
                    <span>{cat.categoryName}</span>
                    <div>
                      <button
                        onClick={() => {
                          setEditingCategoryId(cat.categoryId);
                          setEditCategoryName(cat.categoryName);
                          setEditCategoryImage(null);
                        }}
                        style={{
                          marginRight: 8,
                          background: "#007bff",
                          color: "white",
                          border: "none",
                          padding: "2px 8px",
                          borderRadius: "4px",
                          cursor: "pointer",
                        }}
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => handleDeleteCategory(cat.categoryId)}
                        style={{
                          background: "red",
                          color: "white",
                          border: "none",
                          padding: "2px 8px",
                          borderRadius: "4px",
                          cursor: "pointer",
                        }}
                      >
                        Delete
                      </button>
                    </div>
                  </>
                )}
              </li>
              {cat.subCategories && cat.subCategories.length > 0 ? (
                cat.subCategories.map((sub) => (
                  <li
                    key={sub.subCategoryId}
                    style={{
                      marginLeft: 24,
                      color: "#ccc",
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "space-between",
                    }}
                  >
                    {editingSubCategoryId === sub.subCategoryId ? (
                      <form
                        onSubmit={async (e) => {
                          e.preventDefault();
                          const formData = new FormData();
                          formData.append("SubCategoryName", editSubCategoryName);
                          formData.append("CategoryId", cat.categoryId);
                          if (editSubCategoryImage) {
                            formData.append("Image", editSubCategoryImage);
                          }
                          const token = localStorage.getItem("token");
                          const res = await fetch(
                            `${apiUrl}/subcategories/${sub.subCategoryId}`,
                            {
                              method: "PUT",
                              headers: { Authorization: `Bearer ${token}` },
                              body: formData,
                            }
                          );
                          if (res.ok) {
                            setEditingSubCategoryId(null);
                            setEditSubCategoryName("");
                            setEditSubCategoryImage(null);
                            fetchCategories();
                          }
                        }}
                        style={{ display: "flex", alignItems: "center", gap: 8, flex: 1 }}
                      >
                        <input
                          type="text"
                          value={editSubCategoryName}
                          onChange={(e) => setEditSubCategoryName(e.target.value)}
                          required
                          style={{
                            border: "none",
                            background: "transparent",
                            color: "white",
                            outline: "none",
                            fontSize: "1em",
                            flex: 1,
                          }}
                        />
                        <input
                          type="file"
                          accept="image/*"
                          onChange={(e) => setEditSubCategoryImage(e.target.files[0])}
                          style={{ color: "white" }}
                        />
                        <button
                          type="submit"
                          style={{
                            background: "green",
                            color: "white",
                            border: "none",
                            padding: "4px 12px",
                            borderRadius: "4px",
                            cursor: "pointer",
                          }}
                        >
                          Save
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            setEditingSubCategoryId(null);
                            setEditSubCategoryName("");
                            setEditSubCategoryImage(null);
                          }}
                          style={{
                            background: "#555",
                            color: "white",
                            border: "none",
                            padding: "4px 12px",
                            borderRadius: "4px",
                            cursor: "pointer",
                          }}
                        >
                          Cancel
                        </button>
                      </form>
                    ) : (
                      <>
                        <span>{sub.subCategoryName}</span>
                        <div>
                          <button
                            onClick={() => {
                              setEditingSubCategoryId(sub.subCategoryId);
                              setEditSubCategoryName(sub.subCategoryName);
                              setEditSubCategoryImage(null);
                            }}
                            style={{
                              marginRight: 8,
                              background: "#007bff",
                              color: "white",
                              border: "none",
                              padding: "2px 8px",
                              borderRadius: "4px",
                              cursor: "pointer",
                            }}
                          >
                            Edit
                          </button>
                          <button
                            onClick={() =>
                              handleDeleteSubCategory(sub.subCategoryId, cat.categoryId)
                            }
                            style={{
                              background: "red",
                              color: "white",
                              border: "none",
                              padding: "2px 8px",
                              borderRadius: "4px",
                              cursor: "pointer",
                            }}
                          >
                            Delete
                          </button>
                        </div>
                      </>
                    )}
                  </li>
                ))
              ) : (
                <li style={{ marginLeft: 24, color: "#888" }}>
                  No subcategories
                </li>
              )}
              <li style={{ marginLeft: 24, marginTop: 8 }}>
                <form
                  onSubmit={async (e) => {
                    e.preventDefault();
                    const formData = new FormData();
                    formData.append("SubCategoryName", subCatInputs[cat.categoryId]?.name || "");
                    formData.append("CategoryId", cat.categoryId);
                    if (subCatInputs[cat.categoryId]?.file) {
                      formData.append("Image", subCatInputs[cat.categoryId].file);
                    }
                    const token = localStorage.getItem("token");
                    const res = await fetch(`${apiUrl}/subcategories`, {
                      method: "POST",
                      headers: { Authorization: `Bearer ${token}` },
                      body: formData,
                    });
                    if (res.ok) {
                      setSubCatInputs((prev) => ({
                        ...prev,
                        [cat.categoryId]: { name: "", file: null },
                      }));
                      fetchCategories();
                    }
                  }}
                  style={{ display: "flex", alignItems: "center", gap: 8 }}
                >
                  <input
                    type="text"
                    placeholder="New subcategory name"
                    required
                    value={subCatInputs[cat.categoryId]?.name || ""}
                    onChange={(e) =>
                      setSubCatInputs((prev) => ({
                        ...prev,
                        [cat.categoryId]: {
                          ...prev[cat.categoryId],
                          name: e.target.value,
                        },
                      }))
                    }
                    style={{
                      border: "none",
                      background: "transparent",
                      color: "white",
                      outline: "none",
                      fontSize: "1em",
                      flex: 1,
                    }}
                  />
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e) =>
                      setSubCatInputs((prev) => ({
                        ...prev,
                        [cat.categoryId]: {
                          ...prev[cat.categoryId],
                          file: e.target.files[0],
                        },
                      }))
                    }
                    style={{ color: "white" }}
                  />
                  <button
                    type="submit"
                    style={{
                      background: "green",
                      color: "white",
                      border: "none",
                      padding: "4px 12px",
                      borderRadius: "4px",
                      cursor: "pointer",
                    }}
                  >
                    Add Subcategory
                  </button>
                </form>
              </li>
            </ul>
          ))}
        </div>
      )}
    </div>
  );
}

export default AdminCategories;