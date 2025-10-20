import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

function UserDetail() {
  const { userName } = useParams();
  const [user, setUser] = useState(null);
  const [error, setError] = useState("");
  const apiUrl = import.meta.env.VITE_API_URL;
  const [categoryImageMap, setCategoryImageMap] = useState({});
  const [subCategoryImageMap, setSubCategoryImageMap] = useState({});

  useEffect(() => {
    fetch(`${apiUrl}/api/users/username/${userName}`)
      .then(res => {
        if (!res.ok) return res.text().then(text => { throw new Error(text); });
        return res.json();
      })
      .then(data => setUser(data))
      .catch(err => setError(err.message));
  }, [userName, apiUrl]);

  useEffect(() => {
    if (!user?.categoryRatingEntities?.length) return;
    const ids = Array.from(
      new Set(user.categoryRatingEntities.map(c => c.categoryId).filter(Boolean))
    );
    if (ids.length === 0) return;

    fetch(`${apiUrl}/api/categories`)
      .then(res => res.ok ? res.json() : [])
      .then(allCats => {
        const map = {};
        allCats.forEach(cat => {
          if (ids.includes(cat.categoryId)) {
            const p = cat.imagePath || "";
            map[cat.categoryId] = p.startsWith("http") ? p : `${apiUrl}/${p}`;
          }
        });
        setCategoryImageMap(map);
      })
      .catch(() => {});
  }, [user, apiUrl]);

  useEffect(() => {
    fetch(`${apiUrl}/api/subcategories`)
      .then(res => (res.ok ? res.json() : []))
      .then(list => {
        const map = {};
        list.forEach(sc => {
          const p = sc.imagePath || "";
          if (p) map[sc.subCategoryId] = p.startsWith("http") ? p : `${apiUrl}/${p}`;
        });
        setSubCategoryImageMap(map);
      })
      .catch(() => {});
  }, [apiUrl]);

  if (error) return <div style={{ color: "red" }}>{error}</div>;
  if (!user) return <div>Loading...</div>;

  const avatarSrc = user.profilePicture
    ? (user.profilePicture.startsWith("http") ? user.profilePicture : `${apiUrl}/${user.profilePicture}`)
    : `${apiUrl}/profile-pictures/default.png`;

  // Best-effort resolver for a category image (fallback to first subcategory image if present)
  const getCategoryImage = (cat) => {
    const fromMap = categoryImageMap[cat.categoryId];
    if (fromMap) return fromMap;

    const path = cat.categoryImagePath

    if (!path) return null;
    return path.startsWith("http") ? path : `${apiUrl}/${path}`;
  };

  const categories = user.categoryRatingEntities || [];

  return (
    <div className="user-detail">
      <div className="user-profile-header">
        <h1 className="user-name-title">{user.userName}</h1>
        <div className="avatar-outer">
          <img
            src={avatarSrc}
            alt={`${user.userName} avatar`}
            className="avatar-circle-fade"
          />
        </div>
        
      </div>

      <h2 className="section-title">Ratings</h2>

      {categories.length > 0 ? (
        <div className="category-grid">
          {categories.map(cat => {
            const img = getCategoryImage(cat);
            return (
              <div key={cat.categoryId ?? cat.categoryName} className="category-card">
                
                {img ? (
                  <img className="category-image" src={img} alt={cat.categoryName} />
                ) : (
                  <div className="category-image placeholder" aria-hidden="true">
                    {(cat.categoryName?.[0] || "?").toUpperCase()}
                  </div>
                )}

                <div className="category-title">{cat.categoryName}</div>

                {cat.subCategoryRatingEntities?.length > 0 ? (
                  <ul className="subratings">
                    {cat.subCategoryRatingEntities.map(sub => {
                      const subImg = subCategoryImageMap[sub.subCategoryId];
                      return (
                        <li key={sub.subCategoryId ?? sub.subCategoryName} className="subrating-row">
                          <span className="sub-name">
                            {subImg ? (
                              <img
                                className="subcat-icon"
                                src={subImg}
                                alt={`${sub.subCategoryName} icon`}
                                loading="lazy"
                              />
                            ) : (
                              <span className="subcat-icon placeholder" aria-hidden="true">
                                {(sub.subCategoryName?.[0] || "?").toUpperCase()}
                              </span>
                            )}
                            <span className="subcat-label">{sub.subCategoryName}</span>
                          </span>
                          <span className="sub-score">{sub.rating}</span>
                        </li>
                      );
                    })}
                  </ul>
                ) : (
                  <div className="empty-note">No subcategory ratings</div>
                )}
              </div>
            );
          })}
        </div>
      ) : (
        <div className="empty-note" style={{ textAlign: "center" }}>No ratings yet.</div>
      )}
    </div>
  );
}

export default UserDetail;