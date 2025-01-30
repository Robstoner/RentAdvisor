import "../css/PropertyCard.css";
import placeholderImage from "../assets/bg.png";
import { Property } from "../App.tsx"
import axios from "../api/axiosConfig.ts"
import { useEffect, useState } from "react";

const GET_PHOTO_URL = "api/";

type PropertyCardProps = {
    property: Property;
};

const PropertyCard: React.FC<PropertyCardProps> = ({ property }) => {

    const [imageUrl, setImageUrl] = useState<string>(placeholderImage);

    useEffect(() => {
        if (property.photos && property.photos.length > 0) {
            fetchImage(property.photos[0].id);
        }
    }, [property.photos]);

    const fetchImage = async (photoId: string) => {
        try {
            const response = await axios.get(`/api/Properties/photos/${photoId}`, {
                responseType: "blob",
            });
            const imageBlob = new Blob([response.data], { type: response.headers["content-type"] });
            const imageObjectURL = URL.createObjectURL(imageBlob);
            setImageUrl(imageObjectURL);
        } catch (error) {
            console.error("Error fetching image:", error);
        }
    };
    console.log(imageUrl);
    
    return (
        <div className="property-card">
            <div
                className="property-left">
                <div className="property-image"
                    style={{
                        backgroundImage: `url(${imageUrl})`,
                    }}
                >
                </div>
            </div>
            <div className="property-right">
                <div className="property-title">
                    <div className="property-name">{property.name}</div>
                    <div className="property-address">{property.address}</div>
                </div>
                <div className="propertybody">
                    {/* <p className="property-description">{property.description}</p> */}
                </div>
                <div className="property-features">
                    <ul>
                        {property.features.map((feature, index) => (
                            <li key={index} className="property-feature">{feature}</li>
                        ))}
                    </ul>
                </div>
                <p className="property-dates">
                    Listed on: {new Date(property.createdAt).toLocaleDateString()} <br />
                    Last updated: {new Date(property.updatedAt).toLocaleDateString()}
                </p>
            </div>
        </div>
    );
};

export default PropertyCard;