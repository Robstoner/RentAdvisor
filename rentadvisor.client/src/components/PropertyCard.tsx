import "../css/PropertyCard.css";
import placeholderImage from "../assets/bg.png";

type Property = {
    id: string;
    name: string;
    address: string;
    description: string;
    features: string[];
    images?: string[];
    createdAt: string;
    updatedAt: string;
};

type PropertyCardProps = {
    property: Property;
};

const PropertyCard: React.FC<PropertyCardProps> = ({ property }) => {
    const imageUrl =
        property.images && property.images.length > 0
            ? property.images[0]
            : placeholderImage;
    
    return (
        <div className="property-card">
            <div
                className="property-left"
                style={{
                    backgroundImage: `url(${imageUrl})`,
                }}
            ></div>
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
