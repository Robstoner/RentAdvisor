import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom"; // Import useNavigate for navigation
import axios from "../api/axiosConfig";
import PropertyCard from "../components/PropertyCard";
import "../css/PropertyCard.css";
import "../css/Home.css"
import "../css/CreateProperty.css"

import { Property } from "../App.tsx";


const Home: React.FC = () => {
    const [properties, setProperties] = useState<Property[] | "">("");
    const [loading, setLoading] = useState<boolean>(true);
    const navigate = useNavigate(); // Initialize the useNavigate hook

    useEffect(() => {
        // Fetch properties from the API
        const fetchProperties = async () => {
            try {
                const response = await axios.get<Property[]>("/api/Properties");
                console.log(response.data); // Debug API response

                setProperties(response.data);
            } catch (error) {
                console.error("Failed to fetch properties", error);
            } finally {
                setLoading(false);
            }
        };

        fetchProperties();
    }, []);

    const handleCreateProperty = () => {
        navigate("/createProperty"); // Redirect to the createProperty page
    };

    const handlePropertyClick = (propertyId: string) => {
        navigate(`/property/${propertyId}`); // Redirect to the PropertyDetails page
    };

    return (
        <div className="home-container">
            <div className="home-header">
                <h2>Rent Advisor</h2>
            </div>
            <div className="home-content">
                <div className="button-container">
                    <button className='create-button' type="button" onClick={handleCreateProperty}>Create Property</button>
                </div>
                {loading ? (
                    <div>Loading properties...</div>
                ) : Array.isArray(properties) ? (
                    <div className="properties-container">
                        {properties.map((property) => (
                            <div
                                key={property.id}
                                onClick={() => handlePropertyClick(property.id)}
                                className="properties-card"
                            >
                                <PropertyCard
                                    key={property.id}
                                    property={property}
                                />
                            </div>
                        ))}
                    </div>
                ) : (
                    <div>No properties available</div>
                )}
            </div>
        </div>
    );
};

export default Home;